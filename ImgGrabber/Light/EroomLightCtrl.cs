using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImgGrabber
{
    public class EroomLightCtrl : ILightControl
    {
        private readonly SerialPort serialCOM = new SerialPort();

        private readonly object obj = new object();

        public event DelLightReceive ReceiveEvent;
        public event DelLightConnect ConnectEvent;

        private int[] lightTempArray;
        private bool[] lightStateArray;
        private int[] dimmingValueArray;
        private Thread threadUpdateData;
        private Thread threadSendData;
        private readonly ConcurrentQueue<byte[]> queueSendData = new ConcurrentQueue<byte[]>();
        private bool terminate = false;

        public FormMain Parent { get; set; }
        public int LightChannelCount { get; set; }
        public int ComPortNo { get; set; }
        public int MaxValue { get; set; }
        public List<int> InitValue { get; set; }
        public bool CheckTemp { get; set; }
        public bool IsOpen { get; set; }

        public enum Command : byte
        {
            SetBright,
            GetBright,
            SetOnOffEx,
            GetOnOffEx,
            SetOnOff,
            GetOnOff,

            /*
            SaveBright,
            LoadBright,
            SetBrightMem,
            GetBrightMem,
            SaveMem,

            SetTrigOntime,
            GetTrigOntime,
            SetTrigEdge,
            GetTrigEdge,
            SetFreeRun,
            GetFreeRun,
            SaveTrigger,
            LoadTrigger,
            */

            SetMode,
            GetMode,
            GetFwVersion,
            GetModel,
            GetSN,

            GetErrStatus,
            GetOpenLedStatus,
            GetWrnStatus,
            GetTemperature,

        }

        public enum Mode : int
        {
            Manual = 0,
            Remote = 1
        }

        public void Dispose()
        {
            terminate = true;

            if (serialCOM.IsOpen)
            {
                if (threadUpdateData.IsAlive)
                {
                    threadUpdateData.Abort();
                }

                serialCOM.DataReceived -= new SerialDataReceivedEventHandler(ReadData);

                Thread serialClose;
                serialClose = new Thread(new ThreadStart(serialDisConnect));
                serialClose.Start();
            }
        }

        private void serialDisConnect()
        {
            try
            {
                serialCOM.ReadExisting(); // 시리얼 읽기, 비우기
                serialCOM.DiscardInBuffer();
                serialCOM.DiscardOutBuffer();
                serialCOM.Close(); // 시리얼 포트 닫기
            }
            catch { }
        }

        public bool InitLight()
        {
            try
            {
                if (LightChannelCount > 0)
                {
                    lightTempArray = new int[LightChannelCount];
                    lightStateArray = new bool[LightChannelCount];
                    dimmingValueArray = new int[LightChannelCount];
                }

                if (ComPortNo != 0)
                {
                    string strComPort = string.Format("COM{0:d}", ComPortNo);
                    serialCOM.PortName = strComPort;
                    serialCOM.Parity = 0;
                    serialCOM.BaudRate = 38400;
                    serialCOM.DataBits = 8;
                    serialCOM.StopBits = (StopBits)1;
                    serialCOM.NewLine = "\n";

                    if (!serialCOM.IsOpen)
                    {
                        serialCOM.Open();
                        serialCOM.DataReceived += new SerialDataReceivedEventHandler(ReadData);

                        ConnectEvent?.Invoke(true);

                        SetMode(Mode.Remote);

                        LightAllOnOff(true);
                        Thread.Sleep(500);
                        LightAllOnOff(false);

                        Parent.Logging($"Light Control Intialize [COM{ComPortNo}]");
                    }
                }

                threadUpdateData = new Thread(UpdateLight)
                {
                    IsBackground = true
                };
                threadUpdateData.Start();


                threadSendData = new Thread(SendQueueData)
                {
                    IsBackground = true
                };
                threadSendData.Start();

                IsOpen = true;
                return true;
            }
            catch
            {
                ConnectEvent?.Invoke(false);
                IsOpen = false;
                return false;

            }
        }



        public void LightAllOnOff(bool onOff)
        {
            if (serialCOM.IsOpen)
            {
                for (int ch = 1; ch <= LightChannelCount; ch++)
                {
                    LightOnOff(ch, onOff);
                }
            }
        }


        public void LightOnOff(int channel, bool onOff)
        {
            if (serialCOM.IsOpen)
            {
                if (channel == 0)
                {
                    LightAllOnOff(onOff);
                }
                else
                {
                    WriteData(serialCOM.PortName, ConvertERI(Command.SetOnOff, channel, onOff));
                }
            }
        }

        public void SetLightValue(int channel, int value)
        {
            if (serialCOM.IsOpen)
            {
                if (channel == 0)
                {
                    //ALL Channel
                }
                else
                {
                    if (channel <= LightChannelCount)
                    {
                        if (value < 0)
                        {
                            value = 0;
                        }

                        if (MaxValue < value)
                        {
                            value = MaxValue;
                        }

                        WriteData(serialCOM.PortName, ConvertERI(Command.SetBright, channel, value));
                        Parent.Logging($"Set Light Value [CH:{channel} Value:{value}]");
                    }
                }


            }
        }

        public void SetMode(Mode mode = Mode.Remote)
        {
            if (serialCOM.IsOpen)
            {
                WriteData(serialCOM.PortName, ConvertERI(Command.SetMode, 0, mode));
            }
        }

        public void UpdateLightState()
        {
            if (serialCOM.IsOpen)
            {
                for (int i = 1; i <= LightChannelCount; i++)
                {
                    WriteData(serialCOM.PortName, ConvertERI(Command.GetOnOff, i));
                }
            }
        }

        public void UpdateLightValue()
        {
            if (serialCOM.IsOpen)
            {
                for (int i = 1; i <= LightChannelCount; i++)
                {
                    WriteData(serialCOM.PortName, ConvertERI(Command.GetBright, i));
                }
            }
        }

        public void UpdateTemp()
        {
            if (serialCOM.IsOpen)
            {
                for (int i = 1; i <= LightChannelCount; i++)
                {
                    WriteData(serialCOM.PortName, ConvertERI(Command.GetTemperature, i));
                }
            }
        }

        public int GetLightTemp(int ch)
        {
            return lightTempArray[ch - 1];
        }

        public bool GetLightState(int ch)
        {
            return lightStateArray[ch - 1];
        }

        public int GetDimmingValue(int ch)
        {
            return dimmingValueArray[ch - 1];
        }

        private byte[] ConvertERI(Command command, int channel = 0, object Parameter = null)
        {
            string str = command.ToString().ToLower();
            switch (command)
            {
                case Command.SetBright:
                case Command.SetOnOff:
                    {
                        if (channel > 0)
                        {
                            channel--;//채널 시작은 0부터 시작 하기에 변경
                            str += ' ' + channel.ToString();
                        }

                        if (Parameter is int prm)
                        {
                            str += ' ' + prm.ToString();
                        }
                        else if (Parameter is bool onoff)
                        {
                            if (onoff)
                            {
                                str += ' ' + 1.ToString();
                            }
                            else
                            {
                                str += ' ' + 0.ToString();
                            }
                        }
                    }
                    break;
                case Command.SetMode:
                case Command.SetOnOffEx:
                    {
                        if (Parameter is bool onoff)
                        {
                            if (onoff)
                            {
                                str += ' ' + 1.ToString();
                            }
                            else
                            {
                                str += ' ' + 0.ToString();
                            }
                        }
                        else if (Parameter is Mode mode)
                        {
                            str += ' ' + ((int)mode).ToString();
                        }
                    }
                    break;

                case Command.GetBright:
                case Command.GetOnOff:
                case Command.GetTemperature:
                    {
                        if (channel > 0)
                        {
                            channel--;
                            str += ' ' + channel.ToString();
                        }
                    }
                    break;
                case Command.GetOnOffEx:
                case Command.GetMode:
                case Command.GetFwVersion:
                case Command.GetModel:
                case Command.GetSN:
                case Command.GetErrStatus:
                case Command.GetOpenLedStatus:
                case Command.GetWrnStatus:
                    break;
                default:
                    break;
            }

            byte[] srcBytes = Encoding.UTF8.GetBytes(str);
            byte[] dstBytes = new byte[srcBytes.Length + 1];

            Buffer.BlockCopy(srcBytes, 0, dstBytes, 0, srcBytes.Length);

            Buffer.SetByte(dstBytes, dstBytes.Length - 1, 0x0A);

            return dstBytes;
        }

        private void WriteData(string strPort, byte[] data)
        {
            if (serialCOM.IsOpen)
            {
                if (!SerialPort.GetPortNames().ToList().Contains(strPort))
                {
                    return;
                }

                queueSendData.Enqueue(data);
            }
        }
        private void ReadData(object sender, SerialDataReceivedEventArgs e)
        {
            lock (obj)
            {
                SerialPort port = sender as SerialPort;

                if (port != null)
                {
                    List<string> listStrData = new List<string>();

                    try
                    {
                        while (port.BytesToRead > 0)
                        {
                            listStrData.Add(port.ReadLine());
                        }

                        foreach (string data in listStrData)
                        {
                            ParsingBuffer(data);
                        }
                    }
                    catch
                    {
                        Parent.Logging($"Failed to receive light control [Data Length : {port.BytesToRead}]");
                    }
                }

            }

        }


        private void ParsingBuffer(string data)
        {
            //if (data.Length == bufferLen - 1)
            {
                string[] subs = data.Split();

                switch (subs[0])
                {
                    case "getbright":
                        {
                            if (subs.Length == 3)
                            {
                                if (int.TryParse(subs[1], out int channel))
                                {
                                    if (int.TryParse(subs[2], out int bright))
                                    {
                                        dimmingValueArray[channel] = Convert.ToInt32(bright);
                                        ReceiveEvent?.Invoke(Command.GetBright);
                                    }
                                }
                            }
                        }
                        break;
                    case "getonoff":
                        {
                            if (subs.Length == 3)
                            {
                                if (int.TryParse(subs[1], out int channel))
                                {
                                    if (int.TryParse(subs[2], out int value))
                                    {
                                        lightStateArray[channel] = value == 1;
                                        ReceiveEvent?.Invoke(Command.GetOnOff);
                                    }
                                }

                            }
                        }
                        break;
                    case "gettemperature":
                        {
                            if (subs.Length == 3)
                            {
                                if (int.TryParse(subs[1], out int channel))
                                {
                                    if (double.TryParse(subs[2], out double temp))
                                    {
                                        lightTempArray[channel] = (int)temp;
                                        ReceiveEvent?.Invoke(Command.GetTemperature);
                                    }
                                }
                            }
                        }
                        break;

                    case "invalid":
                    case "unknown":
                    default:
                        {
                            Parent.Logging(data);
                        }
                        break;
                }
            }
        }

        private void SendQueueData()
        {
            try
            {
                while (true)
                {
                    if (terminate)
                    {
                        break;
                    }

                    if (!queueSendData.IsEmpty)
                    {
                        if (queueSendData.TryDequeue(out byte[] data))
                        {
                            if (serialCOM.IsOpen)
                            {
                                Thread.Sleep(100);
                                serialCOM.Write(data, 0, data.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Parent.Logging(e.Message);
            }
        }

        private void UpdateLight()
        {
            while (true)
            {
                if (terminate)
                {
                    break;
                }

                Thread.Sleep(2000);
                if (serialCOM.IsOpen)
                {
                    if (CheckTemp)
                    {
                        UpdateTemp();
                    }
                    UpdateLightState();
                    UpdateLightValue();
                }
            }
        }

        public void ChangeValue(List<int> vs)
        {
            int ch = 1;
            foreach (int v in vs)
            {
                SetLightValue(ch++, v);
            }
        }
    }
}
