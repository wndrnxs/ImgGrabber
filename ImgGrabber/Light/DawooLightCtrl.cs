using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace ImgGrabber
{

    public class DawooLightCtrl : ILightControl
    {
        private readonly SerialPort serialCOM = new SerialPort();

        private readonly object obj = new object();

        private Thread threadUpdateData;
        private Thread threadSendData;

        private int[] lightTempArray;
        private bool[] lightStateArray;
        private int[] dimmingValueArray;
        private bool terminate = false;
        private readonly ConcurrentQueue<byte[]> queueSendData = new ConcurrentQueue<byte[]>();

        public event DelLightReceive ReceiveEvent;
        public event DelLightConnect ConnectEvent;

        private readonly List<byte> listByteData = new List<byte>();

        public int ComPortNo { get; set; }
        public int LightChannelCount { get; set; }

        public FormMain Parent { get; set; }
        public int MaxValue { get; set; }
        public List<int> InitValue { get; set; }
        public bool CheckTemp { get; set; }
        public bool IsOpen { get; set; }

        public enum LightChannel : byte
        {
            CH1 = 0x01,
            CH2 = 0x02,
            CH3 = 0x03,
            ALL = 0x1f
        }

        public enum Command : byte
        {
            SetDimmingValue = 0x01,
            SetLightOnOff = 0x02,
            GetDimmingValue = 0x11,
            GetLightOnOffState = 0x12,
            GetLightTemperature = 0x13,
            GetVerFirmware = 0x21,
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
                    serialCOM.BaudRate = 9600;
                    serialCOM.DataBits = 8;
                    serialCOM.StopBits = (StopBits)1;

                    if (!serialCOM.IsOpen)
                    {
                        serialCOM.Open();
                        serialCOM.DataReceived += new SerialDataReceivedEventHandler(ReadData);

                        ConnectEvent(true);

                        LightAllOnOff(true);
                        Thread.Sleep(500);
                        LightAllOnOff(false);

                        Parent.Logging($"Light Control Intialize [COM {ComPortNo}]");

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

                        Thread thread = new Thread(new ThreadStart(SaperateBuffer))
                        {
                            IsBackground = true
                        };
                        thread.Start();

                        IsOpen = true;
                        return true;
                    }
                }

                ConnectEvent?.Invoke(false);
                IsOpen = false;
                return false;
            }
            catch
            {
                return false;
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

        public int GetLightTemp(int ch)
        {
            if (ch > LightChannelCount)
            {
                return 0;
            }

            return lightTempArray[ch - 1];
        }

        public bool GetLightState(int ch)
        {
            if (ch > LightChannelCount)
            {
                return false;
            }

            return lightStateArray[ch - 1];
        }

        public int GetDimmingValue(int ch)
        {
            if (ch > LightChannelCount)
            {
                return 0;
            }

            return dimmingValueArray[ch - 1];
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

        public void SetLightValue(int channel, int value)
        {
            if (serialCOM.IsOpen)
            {
                if (channel == 0)
                {
                    if (value < 0)
                    {
                        value = 0;
                    }

                    if (MaxValue < value)
                    {
                        value = MaxValue;
                    }

                    byte[] strData = ConvertDLC((byte)Command.SetDimmingValue, (byte)LightChannel.ALL, value);
                    WriteData(serialCOM.PortName, strData);
                    Parent.Logging($"Set Light Value [CH:ALL Valur:{value}]");

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

                        byte[] strData = ConvertDLC((byte)Command.SetDimmingValue, (byte)channel, value);
                        WriteData(serialCOM.PortName, strData);
                        Parent.Logging($"Set Light Value [CH:{channel} Valur:{value}]");

                    }
                }


            }
        }
        public void LightAllOnOff(bool onOff)
        {
            LightOnOff((byte)LightChannel.ALL, onOff);
        }

        public void LightOnOff(int channel, bool onOff)
        {
            if (serialCOM.IsOpen)
            {
                int Paramter = onOff ? 0x01 : 0x00;
                if (channel == 0)
                {
                    channel = (byte)LightChannel.ALL;
                }
                byte[] strData = ConvertDLC((byte)Command.SetLightOnOff, (byte)channel, Paramter);
                WriteData(serialCOM.PortName, strData);
            }
        }

        public void UpdateTemp()
        {
            if (serialCOM.IsOpen)
            {
                byte[] strData = ConvertDLC((byte)Command.GetLightTemperature);
                WriteData(serialCOM.PortName, strData);
            }
        }

        public void UpdateLightState()
        {
            if (serialCOM.IsOpen)
            {
                byte[] strData = ConvertDLC((byte)Command.GetLightOnOffState);
                WriteData(serialCOM.PortName, strData);
            }
        }

        public void UpdateLightValue()
        {
            if (serialCOM.IsOpen)
            {
                byte[] strData = ConvertDLC((byte)Command.GetDimmingValue);
                WriteData(serialCOM.PortName, strData);
            }
        }

        private byte[] ConvertDLC(byte Command, byte Channel = 0x00, int Paramter = 0x00)
        {
            byte Header = 0xff;
            byte Checksum = 0x00;

            Checksum ^= Header;
            Checksum ^= Command;
            Checksum ^= Channel;

            switch (Command)
            {
                case (byte)DawooLightCtrl.Command.SetDimmingValue:
                    {
                        byte[] buf = new byte[6];

                        byte ParamterH;
                        byte ParamterL;

                        byte[] bts = BitConverter.GetBytes((short)Paramter);
                        ParamterL = bts[0];
                        ParamterH = bts[1];

                        //ParamterH = (byte)(Paramter << 2);
                        //ParamterL = (byte)(Paramter & 0xff);

                        Checksum ^= ParamterH;
                        Checksum ^= ParamterL;

                        buf[0] = Header;
                        buf[1] = Command;
                        buf[2] = Channel;
                        buf[3] = ParamterH;
                        buf[4] = ParamterL;
                        buf[5] = Checksum;
                        //buf = string.Format("{0:c}{1:c}{2:c}{3:c}{4:c}{5:c}", Header, Command, (int)Channel, ParamterH, ParamterL, Checksum);
                        return buf;
                    }
                case (byte)DawooLightCtrl.Command.SetLightOnOff:
                    {
                        byte[] buf = new byte[5];

                        Checksum ^= (byte)Paramter;

                        buf[0] = Header;
                        buf[1] = Command;
                        buf[2] = Channel;
                        buf[3] = (byte)Paramter;
                        buf[4] = Checksum;
                        //buf = string.Format("{0:c}{1:c}{2:c}{3:c}{4:c}", Header, Command, (int)Channel, Paramter, Checksum);
                        return buf;
                    }
                case (byte)DawooLightCtrl.Command.GetDimmingValue:
                case (byte)DawooLightCtrl.Command.GetLightOnOffState:
                case (byte)DawooLightCtrl.Command.GetLightTemperature:
                    {
                        byte[] buf = new byte[3];
                        buf[0] = Header;
                        buf[1] = Command;
                        buf[2] = Checksum;
                        return buf;
                    }
                default:
                    {
                        byte[] buf = new byte[6];
                        return buf;
                    }
            }
        }

        private void WriteData(string strPort, byte[] data)
        {
            lock (obj)
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
                    try
                    {
                        while (port.BytesToRead > 0)
                        {
                            byte[] buffer = new byte[port.BytesToRead];
                            port.Read(buffer, 0, buffer.Length);

                            listByteData.AddRange(buffer);
                        }
                    }
                    catch
                    {
                        Parent.Logging($"Failed to receive light control [Data Length : {port.BytesToRead}]");
                    }
                }
            }
        }

        private void SaperateBuffer()
        {
            while (true)
            {
                ParsingBuffer();
            }
        }

        private void ParsingBuffer()
        {
            try
            {
                if (listByteData.Count >= 3)
                {
                    int startIndex = listByteData.IndexOf(0xff);
                    byte command = listByteData[startIndex + 1];
                    switch (command)
                    {
                        case 0x00:
                            {
                                //State 응답이 필요 없는 경우
                                if (listByteData[startIndex + 2].Equals(0xff))
                                {
                                    listByteData.RemoveRange(startIndex, 3);
                                }
                            }
                            break;
                        case 0x80: //로컬 모드
                            {
                                if (listByteData[startIndex + 2].Equals(0x7f))
                                {
                                    Parent.Logging("Light Control Error : Local mode");
                                    listByteData.RemoveRange(startIndex, 3);
                                }
                            }
                            break;
                        case 0x81:
                            {
                                if (listByteData[startIndex + 2].Equals(0x7e))
                                {
                                    Parent.Logging("Light Control Error : channel error");
                                    listByteData.RemoveRange(startIndex, 3);
                                }
                            }
                            break;
                        case 0x82:
                            {
                                if (listByteData[startIndex + 2].Equals(0x7d))
                                {
                                    Parent.Logging("Light Control Error : command error");
                                    listByteData.RemoveRange(startIndex, 3);
                                }
                            }
                            break;
                        case 0x83:
                            {
                                if (listByteData[startIndex + 2].Equals(0x7c))
                                {
                                    Parent.Logging("Light Control Error : parameter error");
                                    listByteData.RemoveRange(startIndex, 3);
                                }
                            }
                            break;
                        case 0x84:
                            {
                                if (listByteData[startIndex + 2].Equals(0x7b))
                                {
                                    Parent.Logging("Light Control Error : checksum error");
                                    listByteData.RemoveRange(startIndex, 3);
                                }
                            }
                            break;

                        case (byte)Command.GetDimmingValue: //Dimming값 측정
                            {
                                if (listByteData.Count < 7)
                                {
                                    return;
                                }

                                for (int i = 0; i < LightChannelCount; i++)
                                {
                                    byte[] dataCh = { listByteData[startIndex + 3 + i * 2], listByteData[startIndex + 2 + i * 2] };
                                    dimmingValueArray[i] = Convert.ToInt32(BitConverter.ToInt16(dataCh, 0));
                                }

                                byte checksum = 0x00;
                                for (int c = 0; c < 6; c++)
                                {
                                    checksum ^= listByteData[startIndex + c];
                                }

                                if (listByteData[startIndex + 6].Equals(checksum))
                                {
                                    listByteData.RemoveRange(startIndex, 7);
                                }


                                ReceiveEvent?.Invoke(Command.GetDimmingValue);
                            }
                            break;
                        case (byte)Command.GetLightOnOffState: //Dimming값 측정
                            {
                                if (listByteData.Count < 4)
                                {
                                    return;
                                }

                                if (listByteData[startIndex + 2] == 0)
                                {
                                    for (int i = 0; i < LightChannelCount; i++)
                                    {
                                        lightStateArray[i] = false;
                                    }
                                }
                                else
                                {
                                    byte[] data = { listByteData[startIndex + 2] };
                                    BitArray bit8 = new BitArray(data);

                                    for (int i = 0; i < LightChannelCount; i++)
                                    {
                                        lightStateArray[i] = bit8.Get(i);
                                    }
                                }

                                byte checksum = 0x00;
                                for (int c = 0; c < 3; c++)
                                {
                                    checksum ^= listByteData[startIndex + c];
                                }

                                if (listByteData[startIndex + 3].Equals(checksum))
                                {
                                    listByteData.RemoveRange(startIndex, 4);
                                }

                                ReceiveEvent?.Invoke(Command.GetLightOnOffState);
                            }
                            break;

                        case (byte)Command.GetLightTemperature: //온도 측정
                            {
                                if (listByteData.Count < 5)
                                {
                                    return;
                                }

                                for (int i = 0; i < LightChannelCount; i++)
                                {
                                    lightTempArray[i] = Convert.ToInt32(listByteData[startIndex + 2 + i]);
                                }

                                byte checksum = 0x00;
                                for (int c = 0; c < 4; c++)
                                {
                                    checksum ^= listByteData[startIndex + c];
                                }

                                if (listByteData[startIndex + 4].Equals(checksum))
                                {
                                    listByteData.RemoveRange(startIndex, 5);
                                }

                                ReceiveEvent?.Invoke(Command.GetLightTemperature);
                            }
                            break;

                    }
                }
            }
            catch
            {

            }
        }

        private void UpdateLight()
        {
            while (true)
            {
                Thread.Sleep(1000);
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
