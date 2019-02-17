using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace CSRF_emulator
{
    public class FileSystemMonitor
    {
        public delegate void FileSystemEvent(String path);
        public event FileSystemEvent Changed;

        private readonly FileSystemWatcher m_fileSystemWatcher = new FileSystemWatcher();
        private readonly Dictionary<string, DateTime> m_pendingEvents = new Dictionary<string, DateTime>();

        private bool m_timerStarted = false;
        private Timer m_timer;

        public FileSystemMonitor(string dirPath, string filter)
        {
            try
            {
                m_fileSystemWatcher.Path = dirPath;
            }
            catch (ArgumentException e)
            {
                return;
            }

            m_fileSystemWatcher.IncludeSubdirectories = false;
            m_fileSystemWatcher.Filter = filter;
            //设置监视文件的哪些修改行为
            m_fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            m_fileSystemWatcher.Created += new FileSystemEventHandler(OnChange);
            m_fileSystemWatcher.Changed += new FileSystemEventHandler(OnChange);

            m_timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Start()
        {
            m_fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnChange(object sender, FileSystemEventArgs e)
        {
            lock (m_pendingEvents)
            {
                m_pendingEvents[e.FullPath] = DateTime.Now;
                if (!m_timerStarted)
                {
                    m_timer.Change(100, 100);
                    m_timerStarted = true;
                }
            }
        }

        private void OnTimeout(object state)
        {
            List<string> paths;
            lock (m_pendingEvents)
            {
                paths = FindReadyPaths(m_pendingEvents);
                paths.ForEach(delegate (string path)
                {
                    m_pendingEvents.Remove(path);
                });

                if (m_pendingEvents.Count == 0)
                {
                    m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_timerStarted = false;
                }
            }

            paths.ForEach(delegate (string path)
            {
                FireEvent(path);
            });
        }
        private List<string> FindReadyPaths(Dictionary<string, DateTime> events)
        {
            List<string> results = new List<string>();
            DateTime now = DateTime.Now;
            foreach (KeyValuePair<string, DateTime> entry in events)
            {
                double diff = now.Subtract(entry.Value).TotalMilliseconds;
                if (diff >= 75)
                {
                    results.Add(entry.Key);
                }
            }
            return results;
        }

        private void FireEvent(string path)
        {
            FileSystemEvent evt = Changed;
            if (evt != null)
            {
                evt(path);
            }
        }
    }
}
