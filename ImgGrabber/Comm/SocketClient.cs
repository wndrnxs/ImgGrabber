using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestClient
{
    public class SocketClient : IDisposable
    {
        public const uint _HEADERLEN_ = 10;
        public const uint _DATALEN_ = 100;

        private TcpClient tcpClient;
        public delegate void ConnectionDelegate(bool bConnected);
        public delegate void ErrorDelegate(string strErrMsg);
        public delegate void ReceiveDelegate(byte[] strRcv);

        public event ConnectionDelegate ConnectionEvent;
        public event ErrorDelegate ErrorEvent;
        public event ReceiveDelegate RecieveEvent;

        private Thread clientworker;
        private Thread threadAutoConnect;
        private readonly string ip;
        private readonly int port;
        private bool terminate = false;

        public bool IsConected { get; private set; }
        public int RetryCount { get => retryCount; set => retryCount = value; }

        private int connectFailCount;
        private int retryCount = 100;

        public SocketClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            if (threadAutoConnect == null)
            {
                threadAutoConnect = new Thread(AutoConnect)
                {
                    IsBackground = true
                };

                threadAutoConnect.Start();
            }
        }

        public void Dispose()
        {
            Disconnect();
            terminate = true;

            threadAutoConnect.Abort();
            threadAutoConnect = null;
        }

        private void AutoConnect()
        {
            Thread.Sleep(1000);

            while (!terminate)
            {
                Thread.Sleep(50);

                if (tcpClient == null)
                {
                    if (!Connect())
                    {
                        connectFailCount++;

                        if (connectFailCount >= RetryCount)
                        {
                            break;
                        }
                    }
                    else
                    {
                        connectFailCount = 0;
                    }
                }
            }
        }

        public bool Connect()
        {
            tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port));

                clientworker = new Thread(new ParameterizedThreadStart(ClientRecieveThread))
                {
                    IsBackground = true
                };
                clientworker.Start(tcpClient);

                ConnectionEvent?.Invoke(true);
                IsConected = true;
                return true;
            }
            catch (ArgumentNullException exception)
            {
                ErrorEvent?.Invoke(exception.Message);
                IsConected = false;
                Disconnect();
            }
            catch (SocketException exception)
            {
                ErrorEvent?.Invoke(exception.Message);
                IsConected = false;
                Disconnect();
            }
            catch (ObjectDisposedException exception)
            {
                ErrorEvent?.Invoke(exception.Message);
                IsConected = false;
                Disconnect();
            }
            catch (Exception exception)
            {
                ErrorEvent?.Invoke(exception.Message);
                IsConected = false;
                Disconnect();
            }

            return false;
        }

        public void Disconnect()
        {
            try
            {
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                    tcpClient = null;

                    if (clientworker != null)
                    {
                        clientworker.Abort();
                        clientworker = null;
                    }
                }

                ConnectionEvent?.Invoke(false);
            }
            catch
            {

            }
        }
        private void ClientRecieveThread(object sender)
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
                IsConected = false;
                Disconnect();
            }

            IsConected = false;
            Disconnect();
        }

        public void ClientSendMsg(byte[] sndMsg)
        {
            try
            {
                tcpClient.GetStream().Write(sndMsg, 0, sndMsg.Length);
            }
            catch (NullReferenceException)
            {
                //ErrorEvent?.Invoke(exception.Message);
                IsConected = false;
                Disconnect();
            }
            catch (ObjectDisposedException)
            {
                //ErrorEvent?.Invoke(exception2.Message);
                IsConected = false;
                Disconnect();
            }
            catch (IOException)
            {
                //ErrorEvent?.Invoke(exception3.Message);
                IsConected = false;
                Disconnect();
            }
            catch (InvalidOperationException)
            {
                //ErrorEvent?.Invoke(exception4.Message);
                IsConected = false;
                Disconnect();
            }
        }


    }
}
