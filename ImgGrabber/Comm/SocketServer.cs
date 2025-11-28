using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace ImgGrabber
{
    public class SocketServer
    {
        public const uint _HEADERLEN_ = 10;
        public const uint _DATALEN_ = 100;

        private TcpListener Server; // 소켓 서버
        private TcpClient Client; // 클라이언트

        private bool ClientConnected;
        public delegate void ConnectionDelegate(int _nID, bool _bConnected);
        public event ConnectionDelegate ConnectionEvent;
        public delegate void ErrorDelegate(string _strErrMsg);
        public event ErrorDelegate ErrorEvent;
        public delegate void ReceiveDelegate(byte[] _strRcv);
        public event ReceiveDelegate RecieveEvent;

        private readonly Thread trdAutoConnect = null;
        private bool terminate = true;
        private readonly int m_nID = -1;
        private readonly string m_strIp = "127.0.0.1";
        private readonly int m_nPort = 5001;
        private int clientConnectCount = 0;
        public SocketServer(int id, string strIp, int nPort)
        {
            m_strIp = strIp;
            m_nPort = nPort;
            m_nID = id;

            trdAutoConnect = new Thread(new ThreadStart(AutoConnect))
            {
                IsBackground = true
            };

            terminate = false;
        }

        public bool Start()
        {
            Server = new TcpListener(IPAddress.Parse(m_strIp), m_nPort);
            try
            {
                Server.Start(); // 서버 시작

                Thread.Sleep(1);

                //m_trdtcpServerRcv.Start();
                trdAutoConnect.Start();

                return true;
            }
            catch (ArgumentNullException)
            {
                ErrorEvent?.Invoke("An error occurred because remoteEp value is  null.");
            }
            catch (SocketException)
            {
                ErrorEvent?.Invoke("An error occurred when accessing the socket.");
                //소켓에 액세스할 때 오류가 발생했습니다. 자세한 내용은 설명 부분을 참조하세요.
            }
            catch (ObjectDisposedException)
            {
                ErrorEvent?.Invoke("Tcp server is closed.");
            }

            return false;

        }

        public void Terminate()
        {
            trdAutoConnect.Abort();

            terminate = true;

            if (Server != null)
            {
                Server.Stop();
            }

            if (Client != null)
            {
                Client.Close();
            }
        }

        private void ClientThread(object sender)
        {
            try
            {
                TcpClient client = sender as TcpClient;
                int length;
                byte[] buffer = new byte[_HEADERLEN_ + _DATALEN_];

                while ((length = client.GetStream().Read(buffer, 0, buffer.Length)) != 0)
                {
                    byte[] rcvBuffer = new byte[length];
                    Array.Copy(buffer, rcvBuffer, length);
                    RecieveEvent?.Invoke(rcvBuffer);
                }
            }
            catch
            {
                ClientConnected = false;
                ConnectionEvent?.Invoke(m_nID, false);
            }
        }

        private void AutoConnect()
        {
            while (!terminate)
            {
                if (Server != null)
                {
                    if (!ClientConnected)
                    {
                        ConnectionEvent?.Invoke(m_nID, false);

                        Client = Server.AcceptTcpClient(); // 보류중인 클라이언트 연결 수락 , 연결 될때까지 대기

                        clientConnectCount++;

                        if (clientConnectCount > 250)
                        {
                            ErrorEvent?.Invoke($"Error : [clientConnectCount:{clientConnectCount}]");
                        }

                        Thread clientworker = new Thread(new ParameterizedThreadStart(ClientThread))
                        {
                            IsBackground = true
                        };
                        clientworker.Start(Client);

                        Thread.Sleep(50);
                        ClientConnected = true;

                        ConnectionEvent?.Invoke(m_nID, true);
                    }
                    else
                    {
                        if (Client != null)
                        {
                            ClientConnected = IsConnectedClient(Client);
                        }

                    }
                }
                Thread.Sleep(10);

            }
        }

        internal void Send(byte[] buffer)
        {
            try
            {
                if (Client != null)
                {
                    Client.GetStream().Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex.Message);
            }
        }

        public bool IsConnectedClient(TcpClient tcpClient)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections;

            try
            {
                tcpConnections = ipProperties.GetActiveTcpConnections().
                Where(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint) && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint)).ToArray();
            }
            catch
            {
                return false;
            }

            if (tcpConnections != null && tcpConnections.Length > 0)
            {
                TcpState stateOfConnection = tcpConnections.First().State;
                if (stateOfConnection == TcpState.Established)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
