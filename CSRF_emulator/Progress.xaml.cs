using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSRF_emulator
{
    /// <summary>
    /// Progress.xaml 的交互逻辑
    /// </summary>
    public partial class Progress : Window
    {
        public Progress(string head, int progressValue)
        {
            this.Title = head;
            InitializeComponent();
            if (progressValue > 100)
            {
                progress.Value = 0;
            }
            else
            {
                progress.Value = progressValue;
            }

            progress.DataContext = "";
            textBlockUp.Text = "";
            textBlockDown.Text = "";
        }

        public double ProgressVal
        {
            set { progress.Value = value; }
            get { return progress.Value; }
        }

        public string TextBlockUp
        {
            set { textBlockUp.Text = value; }
            get { return textBlockUp.Text; }
        }

        public string TextBlockDown
        {
            set { textBlockDown.Text = value; }
            get { return textBlockDown.Text; }
        }

        private void ProgressWindowClosed(object sender, EventArgs e)
        {
        }
    }
}
