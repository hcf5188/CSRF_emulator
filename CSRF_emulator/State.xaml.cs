using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace CSRF_emulator
{
    public enum Protocol
    {
        DISABLE = 0,
        CAN_15765 = 1,
        CAN_SPECIAL = 2,
        K = 3,
        RS232 = 4,
        RS485 = 5
    }
   
    public class maskAllData
    {
        public string receStr;
        public string sendStr;
    }
    public class kangManyPack   // 处理康明斯的结构体数据
    {
        //public uint no = 0;          // 序号
        public uint packNums = 0;      // 总包数
        public Dictionary<uint, Regex> rg = new Dictionary<uint, Regex>(); //保存多包数据
        public string sendStr;         // 要发送的数据
    }
    public class KMSPackType
    {
        public bool packType;               //康明斯当前数据包的类型  false-单包  true-多包
        public uint currPackNum;            //当前的多包中的第几包，从 1 开始
        public List<uint> lsNo = new List<uint>(); //记录都匹配上了哪些包
    }

    public partial class State : Page
    {
        public State()
        {
            InitializeComponent();
            grid_saffer.AutoGenerateColumns = false;
            tTime.Elapsed += new ElapsedEventHandler(RecTimeOut);
            tTime.AutoReset = true;             //到达时间的时候执行事件； 
            sp.Visibility = Visibility.Hidden;
            
            

        }
        FileSystemMonitor fileSystemMonitor;
        
        void OnChanged(string filePath)
        {
            Dispatcher.Invoke(new Action(delegate
            {


                try
                {
                    XElement doc;
                    string fileFullName = fileDir + fileName;
                    doc = XElement.Load(fileFullName);
                    string proto = doc.Element("PROTOCOL").Value.ToUpper().Replace(" ", "");
                    if (proto == null)
                    {
                        System.Windows.MessageBox.Show("文件内容:缺少协议类型");
                        isFile_OK = false;
                        protocol = Protocol.DISABLE;
                    }
                    else
                    {
                        if (string.Compare(proto, "CAN_15765") == 0)
                            protocol = Protocol.CAN_15765;
                        else if (string.Compare(proto, "CAN_SPECIAL") == 0)
                            protocol = Protocol.CAN_SPECIAL;
                        else if (string.Compare(proto, "K") == 0)
                            protocol = Protocol.K;
                        else if (string.Compare(proto, "RS232") == 0)
                            protocol = Protocol.RS232;
                        else if (string.Compare(proto, "RS485") == 0)
                            protocol = Protocol.RS485;
                        else
                        {
                            protocol = Protocol.DISABLE;//通讯协议不能识别
                            System.Windows.MessageBox.Show("通讯协议不能识别");
                            isFile_OK = false;
                        }
                    }
                    string dataBps = doc.Element("BPS").Value.ToUpper().Replace(" ", "");
                    if (dataBps == null)
                    {
                        System.Windows.MessageBox.Show("文件内容:缺少定义通信波特率");
                        isFile_OK = false;
                        baud = 0;
                    }
                    else
                    {
                        if (string.Compare(dataBps, "1200") == 0)
                            baud = 1200;
                        else if (string.Compare(dataBps, "2400") == 0)
                            baud = 2400;
                        else if (string.Compare(dataBps, "4800") == 0)
                            baud = 4800;
                        else if (string.Compare(dataBps, "9600") == 0)
                            baud = 9600;
                        else if (string.Compare(dataBps, "10400") == 0)
                            baud = 10400;
                        else if (string.Compare(dataBps, "19200") == 0)
                            baud = 19200;
                        else if (string.Compare(dataBps, "28800") == 0)
                            baud = 28800;
                        else if (string.Compare(dataBps, "38400") == 0)
                            baud = 38400;
                        else if (string.Compare(dataBps, "57600") == 0)
                            baud = 57600;
                        else if (string.Compare(dataBps, "115200") == 0)
                            baud = 115200;
                        else if (string.Compare(dataBps, "192000") == 0)
                            baud = 192000;
                        else if (string.Compare(dataBps, "125K") == 0)
                            baud = 125000;
                        else if (string.Compare(dataBps, "250K") == 0)
                            baud = 250000;
                        else if (string.Compare(dataBps, "500K") == 0)
                            baud = 500000;
                        else if (string.Compare(dataBps, "1M") == 0)
                            baud = 1000000;
                        else
                        {
                            baud = 0; //错误的不在此定义的波特率
                            System.Windows.MessageBox.Show("通讯波特率不能识别");
                            isFile_OK = false;
                        }
                    }
                    string cha = doc.Element("PIN").Value.Replace(" ", "");
                    if (cha == null)
                    {
                        if (protocol == Protocol.CAN_15765 || protocol == Protocol.CAN_SPECIAL)
                        {
                            System.Windows.MessageBox.Show("文件内容:缺少通道定义");
                            isFile_OK = false;
                            channel = 0;
                        }
                    }
                    else
                    {
                        if (protocol == Protocol.CAN_15765 || protocol == Protocol.CAN_SPECIAL)
                        {
                            if (string.Compare(cha, "6,14") == 0)
                                channel = 1;
                            else if (string.Compare(cha, "1,9") == 0)
                                channel = 2;
                            else if (string.Compare(cha, "3,11") == 0)
                                channel = 3;
                            else if (string.Compare(cha, "11,12") == 0)
                                channel = 4;
                            else
                            {
                                System.Windows.MessageBox.Show("CAN通道未能识别");
                                isFile_OK = false;
                                channel = 0;
                            }
                        }
                    }
                    if (isFile_OK == true)
                    {
                        dataBlock.Clear();//清空旧数据
                        dataBuf.Clear();
                        maskData.Clear();
                        regexAll.Clear();
                        kmsManyPacks.Clear();

                        IEnumerable<XElement> datasComm;
                        IEnumerable<XElement> datasBlock;
                        datasBlock = doc.Elements("BLOCK");
                        datasComm = doc.Elements("COMM");

                        foreach (XElement block in datasBlock)//提取屏蔽显示的指令
                        {
                            string strBlock = block.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToUpper();
                            int strTrue = 1;
                            try
                            {
                                dataBlock.Add(strBlock, strTrue);
                            }
                            catch
                            {
                                ShowStatus("屏蔽指令重复: " + strBlock);
                            }
                        }
                        uint maskNum = 0;
                        uint kangNo = 0;//多包的序号
                        foreach (XElement comm in datasComm)
                        {
                            XElement send = comm.Element("SEND");
                            XElement rece = comm.Element("RECEIVE");
                            string sStr = send.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToUpper();//去掉字符串中的所有空格
                            string rStr = rece.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToUpper();
                            if (protocol != Protocol.CAN_SPECIAL)
                            {
                                if (rStr.Contains('X') || rStr.Contains("??"))
                                {
                                    string ssStr = rStr.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");//正则表达式
                                    maskAllData mas = new maskAllData();
                                    mas.receStr = rStr;
                                    mas.sendStr = sStr;
                                    Regex rg = new Regex(@ssStr);
                                    regexAll.Add(rg);   //将屏蔽码加入list
                                    maskData.Add(maskNum, mas);
                                    maskNum++;
                                }
                                else
                                {
                                    try
                                    {
                                        dataBuf.Add(rStr, sStr);
                                    }
                                    catch
                                    {
                                        ShowStatus("收发指令重复: " + rece.Value + "  " + send.Value);
                                    }
                                }
                            }
                            else// CAN_SPECIAL 协议
                            {
                                need30 = false;
                                if (rStr.Length < 26)           //  康明斯单包处理
                                {
                                    if (rStr.Contains('X') || rStr.Contains("??"))
                                    {
                                        string ssStr = rStr.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");
                                        maskAllData mas = new maskAllData();
                                        mas.receStr = rStr;
                                        mas.sendStr = sStr;
                                        Regex rg = new Regex(@ssStr);
                                        regexAll.Add(rg);       // 将屏蔽码加入 list
                                        maskData.Add(maskNum, mas);
                                        maskNum++;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            dataBuf.Add(rStr, sStr);
                                        }
                                        catch
                                        {
                                            ShowStatus("收发指令重复: " + rece.Value + "  " + send.Value);
                                        }
                                    }
                                }
                                else//康明斯多包处理
                                {
                                    kangManyPack km = new kangManyPack();
                                    kangNo++;
                                    int indexID = rStr.IndexOf(":");
                                    string canID = rStr.Substring(0, indexID + 1);
                                    rStr = rStr.Substring(indexID + 1);             //提取数据
                                    for (uint i = 1; rStr.Length > 8; i++)
                                    {
                                        string sRece = canID + rStr.Substring(0, 16);
                                        rStr = rStr.Substring(16);
                                        sRece = sRece.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");
                                        Regex rg = new Regex(@sRece);
                                        km.rg.Add(i, rg);
                                        km.packNums++;
                                    }
                                    if (rStr.Length > 0)
                                    {
                                        string sRece = canID + rStr;
                                        sRece = sRece.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");
                                        Regex rg = new Regex(@sRece);
                                        km.rg.Add(km.packNums + 1, rg);
                                        km.packNums++;
                                    }
                                    km.sendStr = sStr;
                                    kmsManyPacks.Add(kangNo, km);
                                }
                            }
                        }
                    }
                    if (isFile_OK == true)
                    {
                        tb_Status.Text = "协议: " + protocol + "  波特率: " + dataBps + "   通道: " + cha + " .";
                        ShowStatus("文件加载成功！");
                        isFile_OK = true;
                        if ((string.Compare(proto, "CAN_15765") == 0) || (string.Compare(proto, "CAN_SPECIAL") == 0))
                        {
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                sp.Visibility = Visibility.Visible;
                            }));
                        }
                        else
                        {
                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                sp.Visibility = Visibility.Hidden;
                            }));
                        }
                    }
                    else
                    {
                        tb_Status.Text = "文件缺少定义或者定义未识别，加载失败！";
                        isFile_OK = false;
                    }
                }
                catch (Exception ae)
                {
                    dataBlock.Clear();
                    dataBuf.Clear();//出现异常，清空数据
                    tb_Status.Text = "文件加载失败";
                    isFile_OK = false;

                    System.Windows.MessageBox.Show(ae.ToString());
                }
                isShow = false;
                isRun = false;
                packNums = 0;
                packNum = 0;
                recNums = 0;       // 接收的总包数
                recNum = 0;        // 接收的当前的包数
                index = 0;
                lengthCur = 0;
            }));
        }
        public void ShowStatus(string ss1)      // 显示状态信息
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                if (tb_Curr.Text.Length > 500)
                    tb_Curr.Text = null;
                tb_Curr.AppendText("\r\n" + " " + ss1);
                tb_Curr.ScrollToEnd();
            }));
        }

        void RecTimeOut(object source, ElapsedEventArgs e)      // 接收时间超时处理函数
        {
            ShowStatus("接收多包超时");
            tTime.Enabled = false;
            recPackNums = 0;                // 多包接收清零
        }
        //将数据显示到  datagrid  中
        void ShowDataGraid(CANDataDISCR ccb)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                grid_saffer.Items.Add(ccb);
                grid_saffer.ScrollIntoView(ccb);
            }));
        }

        private System.Timers.Timer tTime = new System.Timers.Timer(2000);      // alarm，前期调试用
        Dictionary<string, string> dataBuf = new Dictionary<string, string>();  // 应答的数据
        Dictionary<string, int> dataBlock = new Dictionary<string, int>();      // 屏蔽显示的数据
        Dictionary<uint, maskAllData> maskData = new Dictionary<uint, maskAllData>();       // 掩码应答数据
        Dictionary<uint, kangManyPack> kmsManyPacks = new Dictionary<uint, kangManyPack>(); // 保存康明斯多包用


        List<Regex> regexAll = new List<Regex>();                               // 掩码的正则表达式

        public  int baud = 0;
        public byte channel = 0;
        public Protocol protocol = Protocol.DISABLE; // 协议

        public bool mode = true;        // true - 模拟ECU      false - 模拟解码仪
        public bool isFile_OK = false;  // true - 文件打开成功 false - 文件打开失败
        public bool isUSB_OK = false;   // true - 设备连成功   false - 设备连接失败
        public bool isShow = false;     // true - 显示         false - 停止显示数据
        public bool isRun = false;      // true - 程序正常运行 false - 程序停止运行
        public bool isUSBOK = false;    // true - USB连接成功  false - USB连接失败
       
        private void bt_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!isFile_OK)
            {
                ShowStatus("请先打开文件!");
                return;
            }
            else if (!isUSBOK)
            {
                ShowStatus("请用USB连接下位机设备!");
                return;
            }

            isShow = true;//开启显示
            isRun = true;
            byte[] sendData = new byte[64];
            sendData[0] = 0x01;
            sendData[1] = 0x01;
            sendData[2] = (byte)(protocol + 0x10);
            sendData[3] = (byte)(baud & 0x000000FF);
            sendData[4] = (byte)((baud >> 8) & 0x000000FF);
            sendData[5] = (byte)((baud >> 16) & 0x000000FF);
            sendData[6] = channel;
            if (protocol == Protocol.CAN_15765 || protocol == Protocol.CAN_SPECIAL)
            {
                this.Dispatcher.Invoke(new Action(delegate
                {
                    if (rb0.IsChecked == true)
                    {
                        sendData[11] = 1;//数据长度
                        sendData[12] = 1;//没有终端电阻
                    }
                    else if (rb60.IsChecked == true)
                    {
                        sendData[11] = 1;//数据长度
                        sendData[12] = 2;//终端电阻 60 Ω
                    }
                    else
                    {
                        sendData[11] = 1;//数据长度
                        sendData[12] = 3;//终端电阻 120 Ω
                    }
                }));
            }
            try
            {
                if (StaticUSB.myUSB.WriteUSBHID(sendData, 64) != true)// 判断是否发送成功
                {
                    ShowStatus("发送错误，USB连接失败!");
                }
                else
                {
                    if (protocol == Protocol.CAN_15765 || protocol == Protocol.CAN_SPECIAL)
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            if (rb0.IsChecked == true)
                            {
                                ShowStatus("启动运行,无终端电阻!");
                            }
                            else if (rb60.IsChecked == true)
                            {
                                ShowStatus("启动运行,终端电阻60Ω!");
                            }
                            else
                            {
                                ShowStatus("启动运行,无终端电阻120Ω!");
                            }
                        }));
                    }
                    else
                    {
                        ShowStatus("启动运行!");
                    }
                       
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowStatus("发送错误,抛出异常!");
                }));

            }
        }
        
        private void bt_Pause_Click(object sender, RoutedEventArgs e)
        {
            isShow = false;
            ShowStatus("暂停显示，数据正常收发!");
        }

        private void bt_Clear_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                grid_saffer.Items.Clear();
                //清空一切标志变量  从新开始
                dataNo = 1;
                tb_Curr.Text = null;
                need30 = false;
                packNums = 0;      
                packNum = 0;
            }));

        }
        public int aab;
        public string fileDir;
        public string fileName;
        private void bt_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = "请选择 Xml 文件";
            openFile.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;  // 程序所在文件夹
            openFile.Filter = "xml文件(*.xml)|*.xml";
            isFile_OK = true;
            if (openFile.ShowDialog() != System.Windows.Forms.DialogResult.OK)  // 文件打开失败
            {
                isFile_OK = false;
                isShow = false;
                isRun = false;
                packNums = 0;
                packNum = 0;
                recNums = 0;       // 接收的总包数
                recNum = 0;        // 接收的当前的包数
                index = 0;
                lengthCur = 0;
                tb_Status.Text = "文件加载失败";
                System.Windows.MessageBox.Show("文件打开失败，请重新打开！");
                return;
            }
            try
            {
                XElement doc;
                doc = XElement.Load(openFile.FileName);
                
                string openfilename = openFile.FileName;
                aab = openfilename.LastIndexOf("\\");
                fileDir = openfilename.Substring(0,aab+1);
                fileName = openfilename.Substring(aab + 1,openfilename.Length-aab-1);
                string proto = doc.Element("PROTOCOL").Value.ToUpper().Replace(" ", "");
                if (proto == null)
                {
                    System.Windows.MessageBox.Show("文件内容:缺少协议类型");
                    isFile_OK = false;
                    protocol = Protocol.DISABLE;
                }
                else
                {
                    if (string.Compare(proto, "CAN_15765") == 0)
                        protocol = Protocol.CAN_15765;
                    else if (string.Compare(proto, "CAN_SPECIAL") == 0)
                        protocol = Protocol.CAN_SPECIAL;
                    else if (string.Compare(proto, "K") == 0)
                        protocol = Protocol.K;
                    else if (string.Compare(proto, "RS232") == 0)
                        protocol = Protocol.RS232;
                    else if (string.Compare(proto, "RS485") == 0)
                        protocol = Protocol.RS485;
                    else
                    {
                        protocol = Protocol.DISABLE;//通讯协议不能识别
                        System.Windows.MessageBox.Show("通讯协议不能识别");
                        isFile_OK = false;
                    }
                }
                string dataBps = doc.Element("BPS").Value.ToUpper().Replace(" ", "");
                if (dataBps == null)
                {
                    System.Windows.MessageBox.Show("文件内容:缺少定义通信波特率");
                    isFile_OK = false;
                    baud = 0;
                }
                else
                {
                    if (string.Compare(dataBps, "1200") == 0)
                        baud = 1200;
                    else if (string.Compare(dataBps, "2400") == 0)
                        baud = 2400;
                    else if (string.Compare(dataBps, "4800") == 0)
                        baud = 4800;
                    else if (string.Compare(dataBps, "6400") == 0)
                        baud = 6400;
                    else if (string.Compare(dataBps, "9600") == 0)
                        baud = 9600;
                    else if (string.Compare(dataBps, "10400") == 0)
                        baud = 10400;
                    else if (string.Compare(dataBps, "14400") == 0)
                        baud = 14400;
                    else if (string.Compare(dataBps, "19200") == 0)
                        baud = 19200;
                    else if (string.Compare(dataBps, "28800") == 0)
                        baud = 28800;
                    else if (string.Compare(dataBps, "38400") == 0)
                        baud = 38400;
                    else if (string.Compare(dataBps, "41600") == 0)
                        baud = 41600;
                    else if (string.Compare(dataBps, "48000") == 0)
                        baud = 48000;
                    else if (string.Compare(dataBps, "49600") == 0)
                        baud = 49600;
                    else if (string.Compare(dataBps, "57600") == 0)
                        baud = 57600;
                    else if (string.Compare(dataBps, "67200") == 0)
                        baud = 67200;
                    else if (string.Compare(dataBps, "72000") == 0)
                        baud = 72000;
                    else if (string.Compare(dataBps, "83200") == 0)
                        baud = 83200;
                    else if (string.Compare(dataBps, "99200") == 0)
                        baud = 99200;
                    else if (string.Compare(dataBps, "115200") == 0)
                        baud = 115200;
                    else if (string.Compare(dataBps, "124800") == 0)
                        baud = 124800;
                    else if (string.Compare(dataBps, "166400") == 0)
                        baud = 166400;
                    else if (string.Compare(dataBps, "192000") == 0)
                        baud = 192000;
                    else if (string.Compare(dataBps, "125K") == 0)
                        baud = 125000;
                    else if (string.Compare(dataBps, "249600") == 0)
                        baud = 249600;
                    else if (string.Compare(dataBps, "250K") == 0)
                        baud = 250000;
                    else if (string.Compare(dataBps, "500K") == 0)
                        baud = 500000;
                    else if (string.Compare(dataBps, "1M") == 0)
                        baud = 1000000;
                    else
                    {
                        baud = 0; //错误的不在此定义的波特率
                        System.Windows.MessageBox.Show("通讯波特率不能识别");
                        isFile_OK = false;
                    }
                }
                string cha = doc.Element("PIN").Value.Replace(" ", "");
                if (cha == null)
                {
                    if (protocol == Protocol.CAN_15765 || protocol == Protocol.CAN_SPECIAL)
                    {
                        System.Windows.MessageBox.Show("文件内容:缺少通道定义");
                        isFile_OK = false;
                        channel = 0;
                    }
                }
                else
                {
                    if (protocol == Protocol.CAN_15765 || protocol == Protocol.CAN_SPECIAL)
                    {
                        if (string.Compare(cha, "6,14") == 0)
                            channel = 1;
                        else if (string.Compare(cha, "1,9") == 0)
                            channel = 2;
                        else if (string.Compare(cha, "3,11") == 0)
                            channel = 3;
                        else if (string.Compare(cha, "11,12") == 0)
                            channel = 4;
                        else
                        {
                            System.Windows.MessageBox.Show("CAN通道未能识别");
                            isFile_OK = false;
                            channel = 0;
                        }
                    }
                }
                if (isFile_OK == true)
                {
                    fileSystemMonitor = new FileSystemMonitor(fileDir, fileName);
                    fileSystemMonitor.Start();
                    fileSystemMonitor.Changed += OnChanged;
                    dataBlock.Clear();//清空旧数据
                    dataBuf.Clear();
                    maskData.Clear();
                    regexAll.Clear();
                    kmsManyPacks.Clear();

                    IEnumerable<XElement> datasComm;
                    IEnumerable<XElement> datasBlock;
                    datasBlock = doc.Elements("BLOCK");
                    datasComm = doc.Elements("COMM");
                    
                    foreach (XElement block in datasBlock)//提取屏蔽显示的指令
                    {
                        string strBlock = block.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToUpper();
                        int strTrue = 1;
                        try
                        {
                            dataBlock.Add(strBlock, strTrue);
                        }
                        catch
                        {
                            ShowStatus("屏蔽指令重复: " + strBlock);
                        }
                    }
                    uint maskNum = 0;
                    uint kangNo = 0;//多包的序号
                    foreach (XElement comm in datasComm)
                    {
                        XElement send = comm.Element("SEND");
                        XElement rece = comm.Element("RECEIVE");
                        string sStr = send.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToUpper();//去掉字符串中的所有空格
                        string rStr = rece.Value.Replace(" ", "").Replace("\n", "").Replace("\r", "").ToUpper();
                        if (protocol != Protocol.CAN_SPECIAL)
                        {
                            if (rStr.Contains('X') || rStr.Contains("??"))
                            {
                                string ssStr = rStr.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");//正则表达式
                                maskAllData mas = new maskAllData();
                                mas.receStr = rStr;
                                mas.sendStr = sStr;
                                Regex rg = new Regex(@ssStr);
                                regexAll.Add(rg);   //将屏蔽码加入list
                                maskData.Add(maskNum, mas);
                                maskNum++;
                            }
                            else
                            {
                                try
                                {
                                    dataBuf.Add(rStr, sStr);
                                }
                                catch
                                {
                                    ShowStatus("收发指令重复: " + rece.Value + "  " + send.Value);
                                }
                            }
                        }else// CAN_SPECIAL 协议
                        {
                            need30 = false;
                            if (rStr.Length < 26)           //  康明斯单包处理
                            {
                                if (rStr.Contains('X') || rStr.Contains("??"))
                                {
                                    string ssStr = rStr.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");
                                    maskAllData mas = new maskAllData();
                                    mas.receStr = rStr;
                                    mas.sendStr = sStr;
                                    Regex rg = new Regex(@ssStr);
                                    regexAll.Add(rg);       // 将屏蔽码加入 list
                                    maskData.Add(maskNum, mas);
                                    maskNum++;
                                }
                                else
                                {
                                    try
                                    {
                                        dataBuf.Add(rStr, sStr);
                                    }
                                    catch
                                    {
                                        ShowStatus("收发指令重复: " + rece.Value + "  " + send.Value);
                                    }
                                }
                            }
                            else//康明斯多包处理
                            {
                                kangManyPack km = new kangManyPack();
                                kangNo++;
                                int indexID = rStr.IndexOf(":");
                                string canID = rStr.Substring(0,indexID+1);
                                rStr = rStr.Substring(indexID+1);             //提取数据
                                for (uint i = 1; rStr.Length > 8; i++)
                                {
                                    string sRece = canID + rStr.Substring(0, 16);
                                    rStr = rStr.Substring(16);
                                    sRece = sRece.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");
                                    Regex rg = new Regex(@sRece);
                                    km.rg.Add(i, rg);
                                    km.packNums++;
                                }
                                if (rStr.Length > 0)
                                {
                                    string sRece = canID + rStr;
                                    sRece = sRece.Replace("X", "[0-9a-fA-F]").Replace("??", "[0-9a-fA-F]{0,}$");
                                    Regex rg = new Regex(@sRece);
                                    km.rg.Add(km.packNums+1, rg);
                                    km.packNums++;
                                }
                                km.sendStr = sStr;
                                kmsManyPacks.Add(kangNo,km);
                            }
                        }
                    }
                }
                if (isFile_OK == true)
                {
                    tb_Status.Text = "协议: " + protocol + "  波特率: " + dataBps + "   通道: " + cha + " .";
                    ShowStatus("文件加载成功！");
                    isFile_OK = true;
                    if ((string.Compare(proto, "CAN_15765") == 0) || (string.Compare(proto, "CAN_SPECIAL") == 0))
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            sp.Visibility = Visibility.Visible;
                        }));
                    }
                    else
                    {
                        this.Dispatcher.Invoke(new Action(delegate
                        {
                            sp.Visibility = Visibility.Hidden;
                        }));
                    }
                }
                else
                {
                    tb_Status.Text = "文件缺少定义或者定义未识别，加载失败！";
                    isFile_OK = false;
                }
            }
            catch (Exception ae)
            {
                dataBlock.Clear();
                dataBuf.Clear();//出现异常，清空数据
                tb_Status.Text = "文件加载失败";
               isFile_OK = false;
              
                System.Windows.MessageBox.Show(ae.ToString());
            }
            isShow = false;
            isRun = false;
            packNums = 0;
            packNum = 0;
            recNums = 0;       // 接收的总包数
            recNum = 0;        // 接收的当前的包数
            index = 0;
            lengthCur = 0;
        }

        private void bt_Stop_Click(object sender, RoutedEventArgs e)
        {
            tTime.Enabled = false;//设置是执行一次（false）还是一直执行(true)；
            isShow = false;
            isRun = false;
            packNums = 0;
            packNum = 0;
            recNums = 0;       // 接收的总包数
            recNum = 0;        // 接收的当前的包数
            index = 0;
            lengthCur = 0;
            ShowStatus("各状态复位，程序停止运行!");
        }
        
        byte[] repBuffToDeal = new byte[600];   // 接收 待代处理的数据
        
        byte recNums = 0;       // 接收的总包数
        byte recNum = 0;        // 接收的当前的包数
        int timeStampOld = 0;   // 上一包的时间戳
        int timeStampCur;       // 当前时间戳
        byte proType;           // 交互协议类型
        int index = 0;
        byte lengthCur=0;
        int recCANID;

        string repStrReal;     // 接收 将 16 进制转为字符串在字典中查询
        uint sendCANID;        // 要发送的 CANID
        //******************* 保留发送要显示的数据 ****************/
        int dataNo = 1;        // 数据序号
        string dataID1;
        int dataToShowLen1;
        string dataToShow1 = null;
        //******************* 保留接收要显示的数据 ****************/
        string dataID2;
        int dataToShowLen2;
        string dataToShow2 = null;
        //**************************************/
        byte[] sendData = new byte[64];         // 封包要发送的数据
        public uint recPackNums = 0;            // 接收到的多包数量
        byte[] manyPacks = new byte[100000];    // 应打包
        bool need30 = false;                    // true - wait 30 cmd   false - 直接下发
        int packNums = 0;                       // 要发送的总数据包
        int packNum = 0;                        // 当前发送的包数
        int lastDataLength = 0;

        public static string ToHexString(byte[] bytes, int star, int leng) // 0xae00cf => "AE00CF"

        {
            string hexString = string.Empty;

            if (bytes != null)

            {
                StringBuilder strB = new StringBuilder();
                for (int i = star; i < star + leng; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        public string GetDataToShow(string toShowStr,int star)
        {
            StringBuilder stoShow = new StringBuilder();
            int strLength = toShowStr.Length;
            string ssToShow = toShowStr.ToUpper();
            for (int i = star; i<star + strLength; i++)
            {
                if (i % 2 == 0)
                    stoShow.Append(' ');
                stoShow.Append(ssToShow[i+star]);
            }
            return stoShow.ToString();
        }

        public void ReceDeal(byte[] reportBuff)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                byte[] recBuff = reportBuff;
                recNum = recBuff[1];            //  获取分包数
                if (!isRun)
                    return;
                if (recNum == 0x01)             //  获取总包数
                {
                    index = 0;                  //  拼包索引用
                    recNums = recBuff[0];       //  总包数
                    proType = recBuff[2];       //  接受到的帧类型
                    lengthCur = recBuff[11];    //  记录当前数据长度

                    System.Array.Clear(repBuffToDeal, 0, repBuffToDeal.Length);
                    int i;
                    for (i = 0; i < lengthCur + 10; i++)//
                    {
                        repBuffToDeal[i] = recBuff[i + 2];
                    }
                    index = i;
                }
                else//整合多包数据
                {
                    lengthCur = recBuff[11];        // 记录当前数据长度
                    for (int i = 0; i < lengthCur; i++)
                        repBuffToDeal[index++] = recBuff[i + 12];
                    repBuffToDeal[9] += lengthCur;  // 只有 K、232、485 才有多包可能
                }
                if (recNum >= recNums)
                {
                    switch (proType)
                    {
                        case 1:                     // CAN_15765 接收报文处理
                            CANReceDeal();
                            break;
                        case 2:                     // CAN_SPECIAL 接收报文处理
                            KMSDeal();
                            break;
                        case 3: case 4: case 5:     // 串口接收报文处理
                            UARTReceDeal();
                            break;
                        
                        case 0x81:case 0x82:
                            CANAnswerShow();    // 将发送过的CAN数据显示出来
                            break;
                        case 0x83: case 0x84:   // 收到应答信号，将发送下的 UART 信息显示出来
                        case 0x85:
                            UARTAnswerShow();
                            break;

                        default: break;
                    }
                }
            }));
        }

        public void CANAnswerShow()
        {
            timeStampCur = (repBuffToDeal[8] << 24) + (repBuffToDeal[7] << 16) + (repBuffToDeal[6] << 8) + (repBuffToDeal[5]);//当前时间戳
            if (isShow)
            {
                CANDataDISCR recDataStru = new CANDataDISCR()
                {
                    no = dataNo++,
                    id = dataID1,
                    direc = "发送",
                    rela_time = (double)(timeStampCur - timeStampOld)/100.0,
                    data_len = dataToShowLen1,
                    data_str = dataToShow1
                };
                ShowDataGraid(recDataStru); 
            }
            timeStampOld = timeStampCur;
                    //将接收到的 CAN 报文显示
            if ((need30 == true)||(packNum >= packNums))//没有多包要发送，或者在等待 30 指令
                return;
            CANSendFunc();                      //继续发送数据
        }

        public void UARTAnswerShow()
        {
            timeStampCur = (repBuffToDeal[8] << 24) + (repBuffToDeal[7] << 16) + (repBuffToDeal[6] << 8) + (repBuffToDeal[5]);//当前时间戳
            if (isShow)
            {
                CANDataDISCR recDataStru = new CANDataDISCR()
                {
                    no = dataNo++,
                    id = null,
                    direc = "发送",
                    rela_time = (double)(timeStampCur - timeStampOld) / 100.0,
                    data_len = dataToShowLen1,
                    data_str = dataToShow1
                };
                ShowDataGraid(recDataStru);         //将接收到的 CAN 报文显示 
            }
            timeStampOld = timeStampCur;
        }

        public uint GetCANID(string strToDeal)
        {
            uint canid = 0;
            for (int i = 0; i < 10; i++)
            {
                if (strToDeal[i] == ':')
                    break;
                else if ((strToDeal[i] >= '0') && (strToDeal[i] <= '9'))
                {
                    canid = (canid << 4) + strToDeal[i] - 0x30;
                }
                else if ((strToDeal[i] >= 'A') && (strToDeal[i] <= 'F'))
                {
                    canid = (canid << 4) + strToDeal[i] - 'A' + 10;
                }
                else if ((strToDeal[i] >= 'a') && (strToDeal[i] <= 'f'))
                {
                    canid = (canid << 4) + strToDeal[i] - 'a' + 10;
                }
            }
            return canid;
        }

        private static byte[] strToToHexByte(char[] hexString)
        {
            byte[] returnBytes = new byte[8];
            for (int i = 0; i < 16; i++)
            {
                if (hexString[i] >= '0' && hexString[i] <= '9')
                {
                    returnBytes[i / 2] = (byte)((returnBytes[i / 2] << 4) + hexString[i] - '0');
                }
                else if (hexString[i] >= 'a' && hexString[i] <= 'f')
                {
                    returnBytes[i / 2] = (byte)((returnBytes[i / 2] << 4) + (hexString[i] - 'a') + 10);
                }
                else if (hexString[i] >= 'A' && hexString[i] <= 'F')
                {
                    returnBytes[i / 2] = (byte)((returnBytes[i / 2] << 4) + (hexString[i] - 'A') + 10);
                }
            }
            return returnBytes;
        }

        private static byte[] strToToHexByte(string hexString,int length)
        {
            byte[] returnBytes = new byte[1000];
            for (int i = 0; i < length; i++)
            {
                if (hexString[i] >= '0' && hexString[i] <= '9')
                {
                    returnBytes[i / 2] = (byte)((returnBytes[i / 2] << 4) + hexString[i] - '0');
                }
                else if (hexString[i] >= 'a' && hexString[i] <= 'f')
                {
                    returnBytes[i / 2] = (byte)((returnBytes[i / 2] << 4) + (hexString[i] - 'a') + 10);
                }
                else if (hexString[i] >= 'A' && hexString[i] <= 'F')
                {
                    returnBytes[i / 2] = (byte)((returnBytes[i / 2] << 4) + (hexString[i] - 'A') + 10);
                }
            }
            return returnBytes;
        }
        // 请求多包指令
        public void CANSend30()
        {
            sendData[0] = 1;
            sendData[1] = 1;
            sendData[2] = (byte)protocol;
            sendData[3] = (byte)(baud & 0x000000FF);
            sendData[4] = (byte)((baud >> 8) & 0x000000FF);
            sendData[5] = (byte)((baud >> 16) & 0x000000FF);
            sendData[6] = channel;
            sendData[11] = (byte)(12);
            sendData[12] = (byte)(sendCANID & 0x000000FF);
            sendData[13] = (byte)((sendCANID >> 8) & 0x000000FF);
            sendData[14] = (byte)((sendCANID >> 16) & 0x000000FF);
            sendData[15] = (byte)((sendCANID >> 24) & 0x000000FF);
            sendData[16] = 0x30;
            byte[] datToStr = new byte[8];
            datToStr[0] = 0x30;
            for (int i = 0; i < 7; i++)
            {
                sendData[17 + i] = 0;
                datToStr[i + 1] = 0;
            }
            try
            {
                if (StaticUSB.myUSB.WriteUSBHID(sendData, 64) != true)//判断是否发送成功
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ShowStatus("数据发送失败，请检查USB连接");
                    }));
                }
                else  // 数据下发至 下位机 成功 将下发的数据保存，收到应答包后显示
                {
                    dataToShow1 = BitConverter.ToString(datToStr).Replace("-", " ");//准备好发送的数据用于显示
                    dataToShowLen1 = 8;//数据长度
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    ShowStatus("USB数据下发错误");
                }));
            }
        }
        // CAN 数据发送函数
        public void CANSendFunc()
        {
            byte datLength;
            packNum++;
            if (packNum < packNums)
                datLength = 8;
            else
                datLength = (byte)lastDataLength;
            
            sendData[0] = 1;
            sendData[1] = 1;
            sendData[2] = (byte)protocol;
            sendData[3] = (byte)(baud & 0x000000FF);
            sendData[4] = (byte)((baud >> 8) & 0x000000FF);
            sendData[5] = (byte)((baud >> 16) & 0x000000FF);
            sendData[6] = channel;
            sendData[11] = (byte)(datLength + 4);
            sendData[12] = (byte)(sendCANID & 0x000000FF);
            sendData[13] = (byte)((sendCANID >> 8) & 0x000000FF);
            sendData[14] = (byte)((sendCANID >> 16) & 0x000000FF);
            sendData[15] = (byte)((sendCANID >> 24) & 0x000000FF);
            byte[] datToStr = new byte[datLength];
            for (int i = 0; i < datLength; i++)
            {
                sendData[16 + i] = manyPacks[(packNum - 1) * 8 + i];
                datToStr[i] = manyPacks[(packNum - 1) * 8 + i];
            }

            List<byte> data = new List<byte>(sendData);
            StaticUSB.SendListBufferMutex.WaitOne();
            StaticUSB.SendListBuffer.Add(data);
            StaticUSB.SendListBufferMutex.ReleaseMutex();

            dataToShow1 = BitConverter.ToString(datToStr).Replace("-"," ");//准备好发送的数据用于显示
            dataToShowLen1 = datLength;//数据长度
        }

        public void UARTSendFunc()
        {
            byte datLength;
            sendData[0] = (byte)packNums;
            sendData[2] = (byte)protocol;
            sendData[3] = (byte)(baud & 0x000000FF);
            sendData[4] = (byte)((baud >> 8) & 0x000000FF);
            sendData[5] = (byte)((baud >> 16) & 0x000000FF);
            sendData[6] = channel;
            while(true)
            {
                packNum++;
                if (packNum < packNums)
                    datLength = 52;
                else
                {
                    datLength = (byte)lastDataLength;
                   
                }
                sendData[1] = (byte)packNum;
                sendData[11] = datLength;
                for (int i = 0; i < datLength; i++)
                {
                    sendData[12 + i] = manyPacks[(packNum - 1) * 52 + i];
                }

                List<byte> data = new List<byte>(sendData);
                StaticUSB.SendListBufferMutex.WaitOne();
                StaticUSB.SendListBuffer.Add(data);
                StaticUSB.SendListBufferMutex.ReleaseMutex();

                if (packNum >= packNums)
                    break;
            }
        }
        // CAN_15765 接收处理
        public void CANReceDeal()
        {
            // 将接收到的数据进行显示
            recCANID = (repBuffToDeal[13] << 24) + (repBuffToDeal[12] << 16) + (repBuffToDeal[11] << 8) + (repBuffToDeal[10]);
            if (recCANID > 0x00FFFFFF)
                dataID2 = ToHexString(repBuffToDeal, 13, 1) + ToHexString(repBuffToDeal, 12, 1) + ToHexString(repBuffToDeal, 11, 1) + ToHexString(repBuffToDeal, 10, 1);
            else if (recCANID > 0x0000FFFF)
                dataID2 = ToHexString(repBuffToDeal, 12, 1) + ToHexString(repBuffToDeal, 11, 1) + ToHexString(repBuffToDeal, 10, 1);
            else if (recCANID > 0x000000FF)
                dataID2 = ToHexString(repBuffToDeal, 11, 1) + ToHexString(repBuffToDeal, 10, 1);
            else
                dataID2 = ToHexString(repBuffToDeal, 10, 1);
            string dataRecStr = ToHexString(repBuffToDeal, 14, (repBuffToDeal[9] - 4));
            string recStr = dataID2 + ":" + dataRecStr;       //获取 CANID + Data 的字符串进行在字典中查询
            ////////////////////    对接收到的数据进行显示    //////////////////
            try {
                int pingBi = dataBlock[recStr];
            }catch
            {
                dataToShow2 = GetDataToShow(dataRecStr, 0);     //获取要显示的数据  数据中间加上空格
                dataToShowLen2 = repBuffToDeal[9] - 4;
                timeStampCur = (repBuffToDeal[8] << 24) + (repBuffToDeal[7] << 16) + (repBuffToDeal[6] << 8) + (repBuffToDeal[5]);//当前时间戳
                if (isShow)
                {
                    CANDataDISCR recDataStru = new CANDataDISCR()
                    {
                        no = dataNo++,
                        id = dataID2,
                        direc = "接收",
                        rela_time = (double)(timeStampCur - timeStampOld) / 100.0,
                        data_len = dataToShowLen2,
                        data_str = dataToShow2
                    };
                    ShowDataGraid(recDataStru);         //将接收到的 CAN 报文显示
                }
                timeStampOld = timeStampCur;
            }
            ///////////////////////////////////////////////////////////////////////////////////////////
            if ((repBuffToDeal[14] == 0x30) && (need30 == true) && (packNum < packNums))
            {
                CANSendFunc();
                need30 = false;
                return;
            }
            else if ((repBuffToDeal[14] >= 0x10) && (repBuffToDeal[14] <= 0x1F) && (protocol == Protocol.CAN_15765)) // 15765 多包
            {
                uint dataNums = ((((uint)repBuffToDeal[14] - 0x10) << 8) + repBuffToDeal[15]);
                recPackNums = (dataNums - 6) % 7 == 0 ? ((dataNums - 6) / 7) : (((dataNums - 6) / 7) + 1);
                repStrReal = dataID2 + ":" + dataRecStr;       //获取 CANID + Data 的字符串进行在字典中查询
                CANSend30();
                need30 = false;
                tTime.Enabled = true;//开始计时
            }
            else if ((recPackNums == 0) && (protocol == Protocol.CAN_15765))// 15765普通单包
            {
                repStrReal = dataID2 + ":" + dataRecStr;
            }
            else if (recPackNums > 0)       //接收到的多包的数量
            {
                repStrReal = repStrReal + dataRecStr;
                recPackNums--;
            }
            if (recPackNums == 0)           //接收完毕
            {
                tTime.Enabled = false;
                try
                {
                    string toSend = dataBuf[repStrReal];
                    int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                    if (idLength <= 0)                          //  没有 CANID ，文件输入有问题
                    {
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowStatus("应答报文无 CANID " + repStrReal);
                        }));
                        return;
                    }
                    sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                    if (sendCANID > 0x7FF)
                        sendCANID += 0x80000000;

                    int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                    packNum = 0;            //重置分包数
                    packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                    lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                    if (protocol == Protocol.CAN_15765)          // 需要 30 指令
                        need30 = true;
                    else if (protocol == Protocol.CAN_SPECIAL)   // 不需要 30 指令
                        need30 = false;
                    Array.Clear(manyPacks, 0, manyPacks.Length);          // 清空数组中的数据
                    manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength); // 将数据填充到待发送的数组

                    dataID1 = toSend.Substring(0, toSend.IndexOf(':')); // 获取要发送的 CANID 字符串，用于显示

                    CANSendFunc();          // 发送 CAN 报文
                }
                catch
                {
                    uint maskNum = 0;
                    bool isFind = false;
                    foreach (Regex rg in regexAll)
                    {
                        if (rg.IsMatch(repStrReal))
                        {
                            maskAllData mas = maskData[maskNum];
                            string toSend;
                           
                            char[] charDat = new char[mas.sendStr.Length];
                            int j = 0;
                            for (int i = 0; i < mas.sendStr.Length; i++)
                            {
                                if (mas.sendStr[i] == 'X')
                                {
                                        
                                    for (;j < mas.receStr.Length; j++)
                                    {
                                        if (mas.receStr[j] == 'X')
                                        {
                                            charDat[i] = repStrReal[j];
                                            j++;
                                            break;
                                        }
                                    }
                                }
                                else
                                    charDat[i] = mas.sendStr[i];
                            }
                            toSend = string.Join(",", charDat).Replace(",", "");
                           
                            int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                            if (idLength <= 0)                          //  没有 CANID ，文件输入有问题
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ShowStatus("应答报文无CANID  " + repStrReal);
                                }));
                                return;
                            }
                            sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                            if (sendCANID > 0x7FF)
                                sendCANID += 0x80000000;

                            int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                            packNum = 0;                // 重置分包数
                            packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                            lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                            if (protocol == Protocol.CAN_15765)          // 需要 30 指令
                                need30 = true;
                            else if (protocol == Protocol.CAN_SPECIAL)   // 不需要 30 指令
                                need30 = false;
                            Array.Clear(manyPacks, 0, manyPacks.Length); //清空数组中的数据
                            manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength);   //将数据填充到待发送的数组

                            dataID1 = toSend.Substring(0, toSend.IndexOf(':')); //获取要发送的 CANID 字符串，用于显示

                            CANSendFunc();          //  发送CAN报文
                            isFind = true;
                            break;
                        }
                        maskNum++;
                    }
                    if (!isFind)
                    {
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowStatus("查无此接收数据" + repStrReal);
                        }));
                    }
                   
                }
            }
           
        }
        // 串口接收到数据进行处理
        public void UARTReceDeal()
        {
            // 将接收到的数据进行显示
            dataToShow2 = ToHexString(repBuffToDeal, 10, repBuffToDeal[9]);
            repStrReal = dataToShow2;//获取 UART 字符串在字典中查询
            try
            {
                int pingbi = dataBlock[repStrReal];
            } catch
            {
            ////////////////////    对接收到的数据进行显示    //////////////////
                dataToShow2 = GetDataToShow(dataToShow2, 0);     // 获取要显示的数据  数据中间加上空格
                dataToShowLen2 = repBuffToDeal[9];
                timeStampCur = (repBuffToDeal[8] << 24) + (repBuffToDeal[7] << 16) + (repBuffToDeal[6] << 8) + (repBuffToDeal[5]);//当前时间戳
                if (isShow)
                {
                    CANDataDISCR recDataStru = new CANDataDISCR()
                    {
                        no = dataNo++,
                        id = null,
                        direc = "接收",
                        rela_time = (double)(timeStampCur - timeStampOld) / 100.0,
                        data_len = dataToShowLen2,
                        data_str = dataToShow2
                    };
                    ShowDataGraid(recDataStru);         //将接收到的 CAN 报文显示
                }
                timeStampOld = timeStampCur;
            }
            try
            {
                string toSend = dataBuf[repStrReal];
                int idLength = toSend.Length;           // 数据长度
                if (idLength <= 0)                      // 没有，文件输入有问题
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ShowStatus("文件错误，此报文对应数据为空  " + repStrReal);
                    }));
                    return;
                }

                packNum = 0;                                    //  重置分包数
                packNums = idLength % 104 == 0 ? (idLength / 104) : (idLength / 104) + 1;// 获取总包数
                lastDataLength = idLength % 104 == 0 ? 52 : (idLength % 104) / 2;        // 最后一包数据的长度

                Array.Clear(manyPacks, 0, manyPacks.Length);    //  清空数组中的数据
                manyPacks = strToToHexByte(toSend,idLength);    //  将数据填充到待发送的数组

                dataToShow1 = GetDataToShow(toSend,0);          //  准备好发送的数据用于显示
                dataToShowLen1 = idLength/2;                    //  数据长度

                UARTSendFunc();                                 //  发送 CAN 报文
            }
            catch
            {
                uint maskNum = 0;
                bool isFind = false;
                foreach (Regex rg in regexAll)
                {
                    if (rg == null)                             //   如果没有数据
                    {
                        break;
                    }
                    if (rg.IsMatch(repStrReal))
                    {
                        maskAllData mas = maskData[maskNum];
                        string toSend = mas.sendStr;
                        char[] charDat = new char[mas.sendStr.Length];
                        byte dataSum = 0;
                        int dataSend = 0;
                        int j = 0;
                        for (int i = 0; i < mas.sendStr.Length-2; i++)  //把发送数据处理校验位处理一下
                        {
                            if (i % 2 == 1)
                            {
                                dataSend = (byte)(dataSend << 4) + (byte)(mas.sendStr[i] >= 'A' ? mas.sendStr[i] - 'A' + 10: mas.sendStr[i] - '0');
                                dataSum += (byte)dataSend;
                            }
                            else
                            {
                                dataSend = (byte)(mas.sendStr[i] >= 'A' ? mas.sendStr[i] - 'A' + 10 : mas.sendStr[i] - '0');
                            }
                            if (mas.sendStr[i] != 'X')
                                charDat[i] = mas.sendStr[i];
                            else
                            {
                                for (; j < mas.receStr.Length - 2; j++)
                                {
                                    if (mas.receStr[j] == 'X')
                                    {
                                        charDat[i] = repStrReal[j];
                                        j++;
                                        break;
                                    }
                                }
                            }
                        }
                        charDat[mas.sendStr.Length - 2] = dataSum.ToString("X2")[0];
                        charDat[mas.sendStr.Length - 1] = dataSum.ToString("X2")[1];
                        toSend = string.Join(",", charDat).Replace(",", "");
                        int idLength = toSend.Length;           // 数据长度
                        if (idLength <= 0)                      // 没有，文件输入有问题
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowStatus("文件错误，此报文对应数据为空  " + repStrReal);
                            }));
                            return;
                        }
                        packNum = 0;            //  重置分包数
                        packNums = idLength % 104 == 0 ? (idLength / 104) : (idLength / 104) + 1;// 获取总包数
                        lastDataLength = idLength % 104 == 0 ? 52 : (idLength % 104) / 2;        // 最后一包数据的长度

                        Array.Clear(manyPacks, 0, manyPacks.Length);    //  清空数组中的数据
                        manyPacks = strToToHexByte(toSend, idLength);   //  将数据填充到待发送的数组

                        dataToShow1 = GetDataToShow(toSend, 0);         //  准备好发送的数据用于显示
                        dataToShowLen1 = idLength / 2;                  //  数据长度

                        UARTSendFunc();                                 //  发送 CAN 报文
                        isFind = true;
                        break;
                    }
                    maskNum++;
                }
                if (!isFind)
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        ShowStatus("查无此接收数据" + repStrReal);
                    }));
                }
            }
        }

        KMSPackType packControl = new KMSPackType() { packType = false, currPackNum = 1, };   // 相当于康明斯数据包的名片
        public void KMSDeal()           // 康明斯 CAN 数据处理
        {
            // 将接收到的数据进行显示
            recCANID = (repBuffToDeal[13] << 24) + (repBuffToDeal[12] << 16) + (repBuffToDeal[11] << 8) + (repBuffToDeal[10]);
            if (recCANID > 0x00FFFFFF)
                dataID2 = ToHexString(repBuffToDeal, 13, 1) + ToHexString(repBuffToDeal, 12, 1) + ToHexString(repBuffToDeal, 11, 1) + ToHexString(repBuffToDeal, 10, 1);
            else if (recCANID > 0x0000FFFF)
                dataID2 = ToHexString(repBuffToDeal, 12, 1) + ToHexString(repBuffToDeal, 11, 1) + ToHexString(repBuffToDeal, 10, 1);
            else if (recCANID > 0x000000FF)
                dataID2 = ToHexString(repBuffToDeal, 11, 1) + ToHexString(repBuffToDeal, 10, 1);
            else
                dataID2 = ToHexString(repBuffToDeal, 10, 1);
            string dataRecStr = ToHexString(repBuffToDeal, 14, (repBuffToDeal[9] - 4));
            string recStr = dataID2 + ":" + dataRecStr;       //获取 CANID + Data 的字符串进行在字典中查询
            ////////////////////    对接收到的数据进行显示    //////////////////
            try
            {
                int pingBi = dataBlock[recStr];
            }
            catch
            {
                dataToShow2 = GetDataToShow(dataRecStr, 0);     //获取要显示的数据  数据中间加上空格
                dataToShowLen2 = repBuffToDeal[9] - 4;
                timeStampCur = (repBuffToDeal[8] << 24) + (repBuffToDeal[7] << 16) + (repBuffToDeal[6] << 8) + (repBuffToDeal[5]);//当前时间戳
                if (isShow)
                {
                    CANDataDISCR recDataStru = new CANDataDISCR()
                    {
                        no = dataNo++,
                        id = dataID2,
                        direc = "接收",
                        rela_time = (double)(timeStampCur - timeStampOld) / 100.0,
                        data_len = dataToShowLen2,
                        data_str = dataToShow2
                    };
                    ShowDataGraid(recDataStru);         //将接收到的 CAN 报文显示
                }
                timeStampOld = timeStampCur;
            }
            ///////////////////////////////////////////////////////////////////////////////////////////
            if (!packControl.packType)           //单包模式
            {
                tTime.Enabled = false;
                try
                {
                    string toSend = dataBuf[recStr];
                    int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                    if (idLength <= 0)                          //  没有 CANID 文件输入有问题
                    {
                        Dispatcher.BeginInvoke(new Action(delegate
                        {
                            ShowStatus("应答报文无CANID  " + recStr);
                        }));
                        return;
                    }
                    sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                    if (sendCANID > 0x7FF)
                        sendCANID += 0x80000000;

                    int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                    packNum = 0;            //重置分包数
                    packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                    lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                    if (protocol == Protocol.CAN_15765)          // 需要 30 指令
                        need30 = true;
                    else if (protocol == Protocol.CAN_SPECIAL)   // 不需要 30 指令
                        need30 = false;
                    Array.Clear(manyPacks, 0, manyPacks.Length);          //清空数组中的数据
                    manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength); //将数据填充到待发送的数组

                    dataID1 = toSend.Substring(0, toSend.IndexOf(':')); //获取要发送的 CANID 字符串，用于显示

                    CANSendFunc();          //  发送CAN报文
                }
                catch
                {
                    uint maskNum = 0;
                    bool isFind = false;
                    foreach (Regex rg in regexAll)
                    {
                        if (rg == null)
                        {
                            break;
                        }
                        if (rg.IsMatch(recStr))
                        {
                            maskAllData mas = maskData[maskNum];
                            string toSend;
                            if (recStr.Length < 30)//单包数据要一一对应
                            {
                                char[] charDat = new char[mas.sendStr.Length];
                                for (int i = 0; i < mas.sendStr.Length; i++)
                                {
                                    if (mas.sendStr[i] == 'X')
                                        charDat[i] = recStr[i];
                                    else
                                        charDat[i] = mas.sendStr[i];
                                }
                                toSend = string.Join(",", charDat).Replace(",", "");
                            }
                            else//多包的数据直接发送原始数据
                            {
                                toSend = mas.sendStr;
                            }
                            int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                            if (idLength <= 0)                          //  没有 CANID ，文件输入有问题
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ShowStatus("应答报文无CANID  " + recStr);
                                }));
                                return;
                            }
                            sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                            if (sendCANID > 0x7FF)
                                sendCANID += 0x80000000;

                            int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                            packNum = 0;            //重置分包数
                            packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                            lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                            Array.Clear(manyPacks, 0, manyPacks.Length);          //清空数组中的数据
                            manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength);   //将数据填充到待发送的数组

                            dataID1 = toSend.Substring(0, toSend.IndexOf(':')); //获取要发送的 CANID 字符串，用于显示

                            CANSendFunc();          //  发送CAN报文
                            isFind = true;
                            break;
                        }
                        maskNum++;
                    }
                    if (!isFind)
                    {
                        try
                        {
                            for (uint i = 1;i<= kmsManyPacks.Count; i++)
                            {
                                kangManyPack km = kmsManyPacks[i];
                                if (km.rg[1].IsMatch(recStr))
                                {
                                    packControl.packType = true; //多包
                                    packControl.currPackNum = 1; //第一包数据匹配成功
                                    packControl.lsNo.Add(i);     //将匹配的数据包序号保存，方便后面的比对
                                    isFind = true;               //当前数据匹配了多包的第一包数据
                                }
                            }
                        }
                        catch {
                        }
                        if (!isFind)
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowStatus("查无此接收数据" + recStr);
                            }));
                        }
                    }
                }
            }
            else//已经鉴别出是多包了
            {
                packControl.currPackNum++;
                for(int j=0;j<packControl.lsNo.Count;j++)
                {
                    uint i = packControl.lsNo[j];
                    kangManyPack km = kmsManyPacks[i];
                    if (km.rg[packControl.currPackNum].IsMatch(recStr))
                    {
                        if (packControl.currPackNum >= km.packNums) //包数已经完全匹配成功
                        {
                            packControl.currPackNum = 1;            //匹配成功，开始发送数据
                            packControl.packType = false;           //进入单包状态

                            string toSend = km.sendStr;

                            int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                            if (idLength <= 0)                          //  没有 CANID ，文件输入有问题
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ShowStatus("应答报文无CANID  " + recStr);
                                }));
                                return;
                            }
                            sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                            if (sendCANID > 0x7FF)
                                sendCANID += 0x80000000;

                            int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                            packNum = 0;            //重置分包数
                            packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                            lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                            Array.Clear(manyPacks, 0, manyPacks.Length);          //清空数组中的数据
                            manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength);   //将数据填充到待发送的数组

                            dataID1 = toSend.Substring(0, toSend.IndexOf(':')); //获取要发送的 CANID 字符串，用于显示

                            CANSendFunc();          //  发送CAN报文
                        }
                    }
                    else
                    {
                        packControl.lsNo.Remove(i);
                    }
                }
             
                if (packControl.lsNo.Count == 0)//因为没有匹配成功
                {
                    packControl.currPackNum = 1;            //
                    packControl.packType = false;           //进入单包状态
                    tTime.Enabled = false;
                    try
                    {
                        string toSend = dataBuf[recStr];
                        int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                        if (idLength <= 0)                          //  没有 CANID 文件输入有问题
                        {
                            Dispatcher.BeginInvoke(new Action(delegate
                            {
                                ShowStatus("应答报文无CANID  " + recStr);
                            }));
                            return;
                        }
                        sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                        if (sendCANID > 0x7FF)
                            sendCANID += 0x80000000;

                        int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                        packNum = 0;            //重置分包数
                        packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                        lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                        if (protocol == Protocol.CAN_15765)          // 需要 30 指令
                            need30 = true;
                        else if (protocol == Protocol.CAN_SPECIAL)   // 不需要 30 指令
                            need30 = false;
                        Array.Clear(manyPacks, 0, manyPacks.Length);          //清空数组中的数据
                        manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength); //将数据填充到待发送的数组

                        dataID1 = toSend.Substring(0, toSend.IndexOf(':')); //获取要发送的 CANID 字符串，用于显示

                        CANSendFunc();          //  发送CAN报文
                    }
                    catch
                    {
                        uint maskNum = 0;
                        bool isFind = false;
                        foreach (Regex rg in regexAll)
                        {
                            if (rg.IsMatch(recStr))
                            {
                                maskAllData mas = maskData[maskNum];
                                string toSend;
                                if (recStr.Length < 30)//单包数据要一一对应
                                {
                                    char[] charDat = new char[mas.sendStr.Length];
                                    for (int i = 0; i < mas.sendStr.Length; i++)
                                    {
                                        if (mas.sendStr[i] == 'X')
                                            charDat[i] = recStr[i];
                                        else
                                            charDat[i] = mas.sendStr[i];
                                    }
                                    toSend = string.Join(",", charDat).Replace(",", "");
                                }
                                else//多包的数据直接发送原始数据
                                {
                                    toSend = mas.sendStr;
                                }
                                int idLength = toSend.IndexOf(":", 0, 10);  //  索引 CANID 字节长度
                                if (idLength <= 0)                          //  没有 CANID ，文件输入有问题
                                {
                                    Dispatcher.BeginInvoke(new Action(delegate
                                    {
                                        ShowStatus("应答报文无CANID  " + recStr);
                                    }));
                                    return;
                                }
                                sendCANID = GetCANID(toSend);   // 获取十进制的 CANID
                                if (sendCANID > 0x7FF)
                                    sendCANID += 0x80000000;

                                int datLength = toSend.Remove(0, toSend.IndexOf(':') + 1).Length;           // 获取数据区长度

                                packNum = 0;            //重置分包数
                                packNums = datLength % 16 == 0 ? (datLength / 16) : (datLength / 16) + 1;   // 获取总包数
                                lastDataLength = datLength % 16 == 0 ? 8 : (datLength % 16) / 2;            // 最后一包数据的长度
                                Array.Clear(manyPacks, 0, manyPacks.Length);          //清空数组中的数据
                                manyPacks = strToToHexByte(toSend.Remove(0, toSend.IndexOf(':') + 1), datLength);   //将数据填充到待发送的数组

                                dataID1 = toSend.Substring(0, toSend.IndexOf(':')); //获取要发送的 CANID 字符串，用于显示

                                CANSendFunc();          //  发送CAN报文
                                isFind = true;
                                break;
                            }
                            maskNum++;
                        }
                        if (!isFind)
                        {
                            try
                            {
                                for (uint i = 0; ; i++)
                                {
                                    kangManyPack km = kmsManyPacks[i];
                                    if (km.rg[1].IsMatch(recStr))
                                    {
                                        packControl.packType = true; //多包
                                        packControl.currPackNum = 1; //第一包数据匹配成功
                                        packControl.lsNo.Add(i);     //将匹配的数据包序号保存，方便后面的比对
                                        isFind = true;               //当前数据匹配了多包的第一包数据
                                    }
                                }
                            }
                            catch
                            {
                            }
                            if (!isFind)
                            {
                                Dispatcher.BeginInvoke(new Action(delegate
                                {
                                    ShowStatus("查无此接收数据" + recStr);
                                }));
                            }
                        }
                    }
                }
                else if (packControl.packType == false)//数据完全匹配成功
                {
                    packControl.lsNo.Clear();
                }
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            CANDataDISCR recDataStru = new CANDataDISCR()
            {
                no = dataNo++,
                id = "18DA00FA",
                direc = "接收",
                rela_time = 100.22,
                data_len = 20,
                data_str = "02 1A 94 00 90 87 65 87"
            }; ShowDataGraid(recDataStru);
            CANDataDISCR recDataStru1 = new CANDataDISCR()
            {
                no = dataNo++,
                id = "18DAFA00",
                direc = "发送",
                rela_time = 1354,
                data_len = 4,
                data_str = "06 5A 94 00"
            };
            ShowDataGraid(recDataStru1);         //将接收到的 CAN 报文显示
            this.Dispatcher.Invoke(new Action(delegate
            {
                //uint maskNum = 0;
                byte[] dds = new byte[] {0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x54, 0x78, 0x90 };
                textBox.AppendText("\n   " + ToHexString(dds,0,9));
                /*  foreach (Regex rg in regexAll)
                  {
                      if (rg.IsMatch("06F1:021A"))
                      {
                          string toSend = maskData[maskNum];
                          textBox.AppendText("\n   " + toSend);
                      }
                      else if (rg.IsMatch("06F1:0227570000232322"))
                      {
                          string toSend = maskData[maskNum];
                          textBox.AppendText("\n   " + toSend);
                      }
                      else
                      {
                          textBox.AppendText("\n   " + "不匹配");
                      }
                      maskNum++;

                  }*/
            }));
        }

        private void grid_saffer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private Thread FileExportThread;
        private Progress FileExportProgress;
        private void KeyDownEvent(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & (ModifierKeys.Control)) == (ModifierKeys.Control))
            {
                System.Windows.Forms.SaveFileDialog fbd = new System.Windows.Forms.SaveFileDialog();
                fbd.Filter = "文本文件   (*.txt)|*.txt";
                fbd.ShowDialog();

                if (fbd.FileName != string.Empty)
                {
                    //创建文件导出线程
                    FileExportThread = new Thread(new ParameterizedThreadStart(FileExportProcess));
                    FileExportThread.IsBackground = true;
                    FileExportThread.Start(fbd.FileName);

                    //创建进度条
                    FileExportProgress = new Progress("导出数据", 0);
                   // FileExportProgress.Owner = this;
                    FileExportProgress.Closed += new EventHandler(ExportDataProgressWindowClosed);
                    FileExportProgress.TextBlockUp = "正在导出数据";
                    //显示进度条
                    FileExportProgress.ShowDialog();
                }
            }
        }

        private void ExportDataProgressWindowClosed(object sender, EventArgs e)
        {
            //终止导出数据线程
            FileExportThread.Abort();
        }

        private void FileExportProcess(object fileName)
        {
            //等待进度条加载完成
            while (FileExportProgress == null)
            {
                Thread.Sleep(10);
            }
            //创建TXT文件
            FileStream fs = new FileStream(fileName.ToString(), FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);

            string strHeader = "";

            //获取文件头
            this.grid_saffer.Dispatcher.Invoke(new Action(delegate
            {
                
                strHeader += (string)grid_saffer.Columns[0].Header;
                strHeader += "\t";
                strHeader += (string)grid_saffer.Columns[1].Header;
                strHeader += "\t\t";
                strHeader += (string)grid_saffer.Columns[2].Header;
                strHeader += "\t";
                strHeader += (string)grid_saffer.Columns[3].Header;
                strHeader += "\t";
                strHeader += (string)grid_saffer.Columns[4].Header;
                strHeader += "\t";
                strHeader += (string)grid_saffer.Columns[5].Header;
                strHeader += "\t";

            }));
            //写文件头
            sw.WriteLine(strHeader);

            //写数据
            int dataItemsCount = 0;
            this.grid_saffer.Dispatcher.Invoke(new Action(delegate
            {
                dataItemsCount = grid_saffer.Items.Count;
            }));

            for (int i = 0; i < dataItemsCount; i++)
            {
                string strColumn = "";
                CANDataDISCR experience = null;
                this.grid_saffer.Dispatcher.Invoke(new Action(delegate
                {
                    experience = (CANDataDISCR)grid_saffer.Items[i];
                }));

                strColumn += experience.no;
                strColumn += "\t";
                strColumn += experience.id;
                strColumn += "\t";
                strColumn += experience.direc;
                strColumn += "\t";
                strColumn += experience.rela_time;
                strColumn += "\t\t";
                strColumn += experience.data_len;
                strColumn += "\t";
                strColumn += experience.data_str;
                strColumn += "\t";

                sw.WriteLine(strColumn);

                FileExportProgress.Dispatcher.Invoke(new Action(delegate
                {
                    FileExportProgress.ProgressVal = 100 * i / grid_saffer.Items.Count;
                    FileExportProgress.TextBlockDown = "已完成 " + FileExportProgress.ProgressVal.ToString() + "%";
                }));
            }

            //导出数据完成
            FileExportProgress.Dispatcher.Invoke(new Action(delegate
            {
                FileExportProgress.ProgressVal = 100;
                FileExportProgress.TextBlockUp = "导出数据完成";
            }));

            sw.Close();

            Thread.Sleep(1000);

            //关闭进度条
            FileExportProgress.Dispatcher.Invoke(new Action(delegate
            {
                FileExportProgress.Close();
            }));
        }

        /*********************************************************************************************
         *********************************************************************************************/
    }
    public class CANDataDISCR
    {
        public int no { get; set; }
        public string id { get; set; }
        public string direc { get; set; }
        public double rela_time { get; set; }
        public int data_len { get; set; }
        public string data_str { get; set; }
    }
  
}
