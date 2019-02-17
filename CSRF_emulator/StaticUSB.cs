using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using System.Windows.Threading;

namespace CSRF_emulator
{
     static public class StaticUSB
    {
        static public USBHID myUSB = new USBHID();         //全局 USB 设备
        private static System.Windows.Threading.DispatcherTimer testUSB = new System.Windows.Threading.DispatcherTimer();
        static public bool flagUSBOPEN = false;
        private static Thread recvThread;
        private static Thread sendThread; 
        //发送数据缓存互斥量
        public static Mutex ReceivedListBufferMutex = new Mutex();
        //发送数据缓存互斥量
        public static Mutex SendListBufferMutex = new Mutex();

        public static List<List<byte>> ReceivedListBuffer = new List<List<byte>>();
        public static List<List<byte>> SendListBuffer = new List<List<byte>>();

        static public void Init()                          //初始化定时器
        {
            testUSB.Tick += new EventHandler(TestUSBFun);
            testUSB.Interval = new TimeSpan(0, 0, 0, 1);    //定时时间 天、时、分、秒
            testUSB.Start();
            myUSB.DataReceived += usbHID_DataReceived;     // USB 异步接收事件
            myUSB.DeviceRemoved += usbHID_DeviceRemoved;
        }

        public static void TestUSBFun(object sender, EventArgs e)//定时检查是否有设备在线
        {
            bool flag = false;
            string deviceStr = "abc";
            foreach (string Str in myUSB.GetDeviceList())
            {
                if (Str.Contains("#vid_9527&pid_1002"))
                {
                    flag = true;
                    deviceStr = Str;
                    break;
                }
            }
            if (flag == false) //没检测到全能王设备
                return;
            if (myUSB.OpenUSBHid(deviceStr))//打开 USB 连接
            {
                StaticUSB.flagUSBOPEN = true;
                testUSB.Stop();             //打开后就关闭定时检查

                recvThread = new Thread(USBRecvProcess);
                recvThread.Start();

                //创建发送线程
                sendThread = new Thread(USBSendProcess);
                sendThread.Start();

                MainWindow.sPage.ShowStatus("设备已连接");//todo:   
                MainWindow.sPage.isUSBOK = true;
            }
        }
        //设备拔掉事件
        static public void usbHID_DeviceRemoved(object sender, EventArgs e)
        {
            StaticUSB.flagUSBOPEN = false;//设备未打开
            testUSB.Start();
            MainWindow.sPage.ShowStatus("设备断开");//todo:   //todo: 显示设备不在线
            MainWindow.sPage.isUSBOK = false;
            if (recvThread.IsAlive)
            {
                recvThread.Abort();
            }
            if (sendThread.IsAlive)
            {
                sendThread.Abort();
            }
        }
        static public void USBRecvProcess()
        {
            while (true)
            {
                if (ReceivedListBuffer.Count <= 0)
                {
                    continue;
                }
                List<byte> data = new List<byte>();
                data.AddRange(ReceivedListBuffer[0]);

                ReceivedListBufferMutex.WaitOne();
                ReceivedListBuffer.RemoveAt(0);
                ReceivedListBufferMutex.ReleaseMutex();

                byte[] reportBuff = new byte[64];
                for (int i = 0; i < data.Count; i++)
                {
                    reportBuff[i] = data[i];
                }
                MainWindow.sPage.ReceDeal(reportBuff);
            }

        }
        static public void USBSendProcess()
        {
            while (true)
            {
                if (SendListBuffer.Count <= 0)
                {
                    continue;
                }
                List<byte> data = new List<byte>();
                data.AddRange(SendListBuffer[0]);

                SendListBufferMutex.WaitOne();
                SendListBuffer.RemoveAt(0);
                SendListBufferMutex.ReleaseMutex();
                byte[] sendData = new byte[64];
                for (int i = 0; i < data.Count; i++)
                {
                    sendData[i] = data[i];
                }
                try
                {
                    if (StaticUSB.myUSB.WriteUSBHID(sendData, 64) != true)  //判断是否发送成功
                    {
                        MainWindow.sPage.ShowStatus("数据发送失败，请检查USB连接");
                    } 
                }
                catch
                {
                    MainWindow.sPage.ShowStatus("USB 数据下发错误");
                }
            }
        }
        static public void usbHID_DataReceived(object sender, EventArgs e)
        {
                report myRP = (report)e;
                List<byte> data = new List<byte>(myRP.reportBuff);
                //添加到接收缓存列表中
                ReceivedListBufferMutex.WaitOne();
                ReceivedListBuffer.Add(data);
                ReceivedListBufferMutex.ReleaseMutex();
        }
    }
}
