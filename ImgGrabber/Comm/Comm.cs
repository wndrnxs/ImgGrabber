using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ImgGrabber
{
    public class Comm
    {
        private readonly FormMain parent;
        private SocketServer socketServer = null;
        private DataPacket recievePacket;
        private DataPacket sendPacket;
        private int netID = 1;

        public delegate void ConnectDelegate(bool bConnected);
        public event ConnectDelegate ConnectEvent;

        private enum CMD : ushort
        {
            READY = 101,
            START = 102,
            END = 103,
            TIMESYNC = 104
        }
        private struct ByteDef
        {
            //Direction
            public const byte VTO = 0x01;
            public const byte OTV = 0x10;
            //SendRecv
            public const byte RECV = 0x01;
            public const byte SEND = 0x10;
            //Response
            public const byte ACK = 0x06;
            public const byte NAK = 0x15;
            //etc.
            public const byte NUL = 0x00;
            public const byte SOH = 0x01;
            public const byte STX = 0x02;
            public const byte ENQ = 0x05;

            public static implicit operator byte(ByteDef v)
            {
                return Convert.ToByte(v);
            }
        }

        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        private struct DataPacket
        {
            public Header header;
            public string Data;

            internal void SetDefault(ushort command, byte response, string data, bool send = false)
            {
                Data = data + (char)ByteDef.NUL;
                header.SetDefault((ushort)Data.Length, command, response, send);
            }
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct Header
        {
            /// <summary>
            /// SOH 고정
            /// </summary>
            public byte StartOfHeader;
            /// <summary>
            /// Data Total Length
            /// </summary>
            public ushort Length;
            /// <summary>
            /// command 100 ~
            /// </summary>
            public ushort Command;
            /// <summary>
            /// ACK or NAK
            /// </summary>
            public byte Response;
            /// <summary>
            /// VTO or OTV
            /// </summary>
            public byte Direction;
            /// <summary>
            /// SEND or RECV
            /// </summary>
            public byte SendRecv;
            /// <summary>
            /// ENQ or NUL
            /// </summary>
            public byte Enquiry;
            /// <summary>
            /// STX 고정
            /// </summary>
            public byte StartOfText;

            internal byte[] GetBytes()
            {
                byte[] buffer = new byte[10];
                unsafe
                {
                    fixed (byte* fixed_buffer = buffer)
                    {
                        Marshal.StructureToPtr(this, (IntPtr)fixed_buffer, false);
                    }
                }
                return buffer;
            }

            internal void SetBytes(byte[] bytes)
            {
                int size = Marshal.SizeOf(typeof(Header));
                if (size > bytes.Length)
                {
                    this = default;
                }

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);

                Header result = (Header)Marshal.PtrToStructure(ptr, typeof(Header));
                Marshal.FreeHGlobal(ptr);
                this = result;
            }

            internal void SetDefault(ushort Length, ushort Command, byte Response, bool bSend)
            {
                StartOfHeader = ByteDef.SOH;

                this.Length = (ushort)(Length - 1);
                this.Command = Command;
                this.Response = Response;

                Direction = ByteDef.OTV;

                if (bSend)
                {
                    SendRecv = ByteDef.SEND;
                    Enquiry = ByteDef.ENQ;
                }
                else
                {
                    SendRecv = ByteDef.RECV;
                    Enquiry = ByteDef.NUL;
                }

                StartOfText = ByteDef.STX;
            }
        }


        [DllImport("kernel32.dll")] public static extern uint SetSystemTime(ref SYSTEMTIME lpSystemTime);

        private static void SetPcTimeUpdate(SYSTEMTIME sTime)
        {
            DateTime tmpdate = new DateTime(sTime.wYear, sTime.wMonth, sTime.wDay, sTime.wHour, sTime.wMinute, sTime.wSecond);
            tmpdate = tmpdate.AddHours(-9); //KOR timezone adjust 

            sTime.wDayOfWeek = (ushort)tmpdate.DayOfWeek;
            sTime.wMonth = (ushort)tmpdate.Month;
            sTime.wDay = (ushort)tmpdate.Day;
            sTime.wHour = (ushort)tmpdate.Hour;
            sTime.wMinute = (ushort)tmpdate.Minute;
            sTime.wSecond = (ushort)tmpdate.Second;
            sTime.wMilliseconds = 0;
            SetSystemTime(ref sTime);
        }

        public Comm(object parent)
        {
            this.parent = parent as FormMain;
        }

        public void OpenServer(int netID, string serverIP, int serverPort)
        {
            this.netID = netID;

            socketServer = new SocketServer(netID, serverIP, serverPort);
            socketServer.RecieveEvent += new SocketServer.ReceiveDelegate(ServerRecieve);
            socketServer.ErrorEvent += new SocketServer.ErrorDelegate(ServerErrorMsg);
            socketServer.ConnectionEvent += new SocketServer.ConnectionDelegate(ServerConnect);

            socketServer.Start();
            parent.Logging($"Server Intialize [IP: {serverIP}, Port: {serverPort}]");
        }

        internal void Dispose()
        {
            socketServer.Terminate();
        }

        private void ServerConnect(int _netID, bool _bConnected)
        {
            if (_netID == netID)
            {
                ConnectEvent?.Invoke(_bConnected);
            }
        }

        private void ServerErrorMsg(string _strErrMsg)
        {
            parent.Logging(_strErrMsg);
        }

        private void ServerRecieve(byte[] byteData)
        {
            try
            {
                string strHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} ",
                    byteData[0], byteData[1], byteData[2], byteData[3], byteData[4], byteData[5], byteData[6], byteData[7], byteData[8], byteData[9]);

                int dataLength = byteData.Length - 10;
                byte[] vs = new byte[dataLength];
                Array.Copy(byteData, 10, vs, 0, dataLength);
                string strData = Encoding.UTF8.GetString(vs);
                parent.Logging("Recieve Header >>" + strHeader);

                ParsingBuf(byteData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ParsingBuf(byte[] byteData)
        {
            recievePacket = GetBindAck(byteData);

            Header header = recievePacket.header;
            CMD Command = (CMD)header.Command;
            string data = ContractTrim(recievePacket.Data);

            parent.Logging("Recieve Data >> " + data);
            //parent.Logging("\n");

            bool ret = false;

            if (header.Enquiry == ByteDef.NAK)
            {
                parent.Logging("NK");
                return;
            }

            if (header.Direction == ByteDef.VTO)
            {
                switch (Command) //통신 헤더가 잘 왔는지 확인
                {
                    case CMD.START:
                        {
                            if (header.SendRecv == ByteDef.SEND) // START 처음 받음
                            {
                                ret = true;
                            }
                        }
                        break;

                    case CMD.END:
                        {
                            if (header.SendRecv == ByteDef.RECV) // 보낸 END 에 대한 회신
                            {
                                ret = true;
                            }
                        }
                        break;
                }

                if (ret)
                {
                    switch (Command) //데이터 처리 부
                    {
                        case CMD.START:
                            {
                                ret = false;

                                parent.CellDatas = data;

                                if (parent.CheckReady())
                                {
                                    if (parent.GrabReady()) //Grab 시작
                                    {
                                        ret = true;
                                    }
                                }
                            }
                            break;
                    }
                }

                if (header.Enquiry == ByteDef.ENQ) //회신 부 (회신이 필요할 시 ret으로 결과 Client에게 공유
                {
                    sendPacket.SetDefault((ushort)Command, ret ? ByteDef.ACK : ByteDef.NAK, "");
                    Send(sendPacket);
                }
            }

        }



        public void SendGrabEnd(string cell_id, bool AK = true)
        {
            sendPacket.SetDefault((ushort)CMD.END, AK ? ByteDef.ACK : ByteDef.NAK, cell_id, true);
            Send(sendPacket);
        }

        private void Send(DataPacket sendPacket)
        {
            try
            {
                byte[] buffer = GetBytes_Bind(sendPacket);
                socketServer.Send(buffer);

                string strHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} ",
                    buffer[0], buffer[1], buffer[2], buffer[3], buffer[4], buffer[5], buffer[6], buffer[7], buffer[8], buffer[9]);

                parent.Logging("Send Header >>" + strHeader);

                string strData = ContractTrim(sendPacket.Data);
                parent.Logging("Send Data>> " + strData);
                //parent.Logging("\n");
            }
            catch
            {
                parent.Logging("Send Error [Send()]");
                parent.Process(FormMain.ProcessState.Alarm);
                return;
            }
        }

        private DataPacket GetBindAck(byte[] btfuffer)
        {
            DataPacket packet = new DataPacket();

            MemoryStream ms = new MemoryStream(btfuffer, false);
            BinaryReader br = new BinaryReader(ms);

            packet.header.SetBytes(br.ReadBytes(10));

            packet.Data = Encoding.UTF8.GetString(br.ReadBytes(btfuffer.Length - 10));

            if (packet.header.Length != (ushort)(packet.Data.Length - 1))
            {
                parent.Logging($"Header Length[{packet.header.Length}] and Data Length[{packet.Data.Length}] are different.");
            }

            br.Close();
            ms.Close();

            return packet;
        }

        private byte[] GetBytes_Bind(DataPacket packet)
        {
            int headerSize = packet.header.GetBytes().Length;
            int dataSize = packet.Data.Length;

            byte[] btBuffer = new byte[headerSize + dataSize];

            MemoryStream ms = new MemoryStream(btBuffer, true);
            BinaryWriter bw = new BinaryWriter(ms);

            try
            {
                byte[] btHeader = new byte[headerSize];
                byte[] btData = new byte[dataSize];

                // Header
                btHeader = packet.header.GetBytes();
                bw.Write(btHeader);

                // Data
                Encoding.UTF8.GetBytes(packet.Data, 0, dataSize, btData, 0);
                bw.Write(btData);

                return btBuffer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : {0} [GetBytes_Bind()]", ex.Message.ToString());
                parent.Logging("\n");
                parent.Process(FormMain.ProcessState.Alarm);
                return null;
            }
            finally
            {
                bw.Close();
                ms.Close();
            }
        }

        // 문자열 뒤쪽에 위치한 null 을 제거한 후에 공백문자를 제거한다.
        private string ContractTrim(string source)
        {
            int index = source.IndexOf((char)ByteDef.NUL);
            if (index > -1)
            {
                source = source.Substring(0, index + 1);
            }
            return source.TrimEnd('\0').Trim();//여백 없애기
        }
    }
}
