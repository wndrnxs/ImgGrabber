using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImgGrabber
{
    public class VitFcLightCtrl : ILightControl
    {
        public class IoData
        {
            const ushort maskHigh = 0x00FF;
            const ushort maskLow = 0xFF00;

            private ushort data;
            public ushort Data { get => data; set => data = value; }
            public byte High
            {
                get
                {
                    var data = BitConverter.GetBytes(this.data);
                    return data[1];
                }
                set
                {
                    ushort temp = (ushort)(value << 8);
                    data &= maskHigh;
                    data |= temp;
                }
            }

            public byte Low
            {
                get
                {
                    var data = BitConverter.GetBytes(this.data);
                    return data[0];
                }
                set
                {
                    ushort temp = value;
                    data &= maskLow;
                    data |= temp;
                }
            }


            internal void Set(int index, bool onOff)
            {
                var io = data;
                ushort sh = 1;

                for (uint i = 0; i < index; i++)
                {
                    sh = (ushort)(sh << 1);
                }

                if (onOff)
                {
                    io = (ushort)(io | sh);
                }
                else
                {
                    io = (ushort)(io & ~sh);
                }

                data = io;
            }

            internal bool IsOn(int index)
            {
                var io = data;
                uint sh = 1;

                for (uint i = 0; i < index; i++)
                {
                    sh = sh << 1;
                }

                return (io & sh) > 0;
            }

        }

        private readonly SerialPort serialCOM = new SerialPort();
        private readonly object obj = new object();


        private int[] lightTempArray;
        private bool[] lightStateArray;
        private int[] dimmingValueArray;
        private Thread threadUpdateData;
        private Thread threadSendData;
        private ConcurrentQueue<byte[]> queueSendData = new ConcurrentQueue<byte[]>();
        private bool terminate = false;
        private IoData setState = new IoData();
        private IoData getState = new IoData();

        public FormMain Parent { get; set; }
        public int LightChannelCount { get; set; }
        public int ComPortNo { get; set; }
        public int MaxValue { get; set; }
        public List<int> InitValue { get; set; }
        public bool CheckTemp { get; set; }
        public bool IsOpen { get; set; }

        public event DelLightReceive ReceiveEvent;
        public event DelLightConnect ConnectEvent;

        public enum Command : byte
        {
            ValueChangeAndOn,  
            OnlyValueChange, 
            OnOff,          
            RequestValue,    
            RequestState,   
            reset,
            UseTempSensorAlarm,
            UseFanAlarm
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
                        byte[] data;
                        if (queueSendData.TryDequeue(out data))
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

        private void WriteData(string strPort, byte[] data)
        {
            if (!SerialPort.GetPortNames().ToList().Contains(strPort))
            {
                return;
            }

            if (data[data.Length - 1] != 0)
            {
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
            byte[] bytes = Encoding.UTF8.GetBytes(data);


            if(bytes[0].Equals(Convert.ToByte('R')))
            {
                int channel = bytes[1] - 48;
                if(channel > 0 && channel <= dimmingValueArray.Length)
                {
                    int n1 = bytes[2] - 48;
                    int n2 = bytes[3] - 48;
                    int n3 = bytes[4] - 48;

                    int bright = n1 * 100 + n2 * 10 + n3;
                    dimmingValueArray[channel-1] = Convert.ToInt32(bright);
                    ReceiveEvent?.Invoke(Command.RequestValue);
                }
            }
            else if(bytes[0].Equals(Convert.ToByte('E')))
            {
                int channel = bytes[1] - 48;
                if (channel > 0 && channel <= dimmingValueArray.Length)
                {
                    int n1 = bytes[2] - 48;
                    int n2 = bytes[3] - 48;
                    int n3 = bytes[4] - 48;

                    int bright = n1 * 100 + n2 * 10 + n3;
                    dimmingValueArray[channel-1] = Convert.ToInt32(bright);
                    ReceiveEvent?.Invoke(Command.RequestValue);
                }
            }
            else if (bytes[0].Equals(Convert.ToByte('O')))
            {
                if(bytes[1].Equals(Convert.ToByte('N')) && bytes[2].Equals(Convert.ToByte('F')))
                {
                    getState.High = bytes[3];
                    getState.Low = bytes[4];

                    for (int i = 0; i < LightChannelCount; i++)
                    {
                        lightStateArray[i] = getState.IsOn(i);
                    }
                    
                    ReceiveEvent?.Invoke(Command.RequestState);
                }
            }
            else
            {

            }
            
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
                    serialCOM.NewLine = "\n";

                    if (!serialCOM.IsOpen)
                    {
                        serialCOM.Open();
                        serialCOM.DataReceived += new SerialDataReceivedEventHandler(ReadData);

                        ConnectEvent?.Invoke(true);

                        LightAllOnOff(true);
                        Thread.Sleep(500);
                        LightAllOnOff(false);

                        UseAlarm(Command.UseFanAlarm, false);
                        UseAlarm(Command.UseTempSensorAlarm, false);

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

        private void UseAlarm(Command useAlarm, bool v)
        {
            byte[] strData = ConvertVIT(useAlarm, 0, v);
            WriteData(serialCOM.PortName, strData);
        }

        public void ChangeValue(List<int> vs)
        {
            int ch = 1;
            foreach (int v in vs)
            {
                SetLightValue(ch++, v);
            }
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

                Thread serialClose = new Thread(new ThreadStart(serialDisConnect));
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

        public void LightAllOnOff(bool onOff)
        {
            LightOnOff(0, onOff);
        }

        public void LightOnOff(int channel, bool onOff)
        {
            if (serialCOM.IsOpen)
            {
                byte[] strData = ConvertVIT(Command.OnOff, channel, onOff);
                WriteData(serialCOM.PortName, strData);
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

                        WriteData(serialCOM.PortName, ConvertVIT(Command.OnlyValueChange, channel, value));
                        Parent.Logging($"Set Light Value [CH:{channel} Value:{value}]");

                    }
                }
            }
        }

        private byte[] ConvertVIT(Command command, int channel, object value = null)
        {
            byte[] data = new byte[7];

            switch (command)
            {
                case Command.ValueChangeAndOn:
                    {
                        if (value is int ivalue)
                        {
                            string strData = string.Format($"Cx{ivalue.ToString("000")}\r\n");
                            Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);
                            data.SetValue((byte)(channel + '0'), 1);
                        }
                    }
                    break;
                case Command.OnlyValueChange:
                    {
                        if (value is int ivalue)
                        {
                            string strData = string.Format($"Dx{ivalue.ToString("000")}\r\n");
                            Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);
                            data.SetValue((byte)(channel + '0'), 1);
                        }
                    }
                    break;
                case Command.OnOff:
                    {
                        if (value is bool on && channel >= 0 && channel <= 16)
                        {
                            if (channel == 0)
                            {
                                setState.Data = on ? (ushort)0xffff : (ushort)0x0000;
                            }
                            else
                            {
                                setState.Set(channel - 1, on);
                            }

                            string strData = string.Format("ONFxx\r\n");
                            Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);

                            data.SetValue(setState.High, 3);
                            data.SetValue(setState.Low, 4);
                        }
                    }
                    break;
                case Command.reset:
                    {
                        string strData = "reset\r\n";
                        Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);
                    }
                    break;
                case Command.RequestValue:
                    {
                        string strData = string.Format("RxDAT\r\n");
                        Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);

                        data.SetValue((byte)(channel + '0'), 1);
                    }
                    break;
                case Command.RequestState:
                    {
                        string strData = "ROONF\r\n";
                        Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);
                    }
                    break;
                case Command.UseTempSensorAlarm:
                    {
                        if(value is bool on)
                        {
                            string strData = on ? $"UTEM1\r\n" : $"UTEM0\r\n";
                            //string strData = "UTEMx\r\n";
                            Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);

                            //data.SetValue(on ? (byte)1 : (byte)0, 4);
                        }
                    }
                    break;
                case Command.UseFanAlarm:
                    {
                        if (value is bool on)
                        {
                            string strData = on ? $"UFAN1\r\n" : $"UFAN0\r\n";
                            //string strData = "UFANx\r\n";
                            Array.Copy(Encoding.UTF8.GetBytes(strData), data, data.Length);

                            //data.SetValue(on ? (byte)1 : (byte)0, 4);
                        }
                    }
                    break;
            }

            return data;
        }

        public void UpdateLightState()
        {
            if (serialCOM.IsOpen)
            {
                WriteData(serialCOM.PortName, ConvertVIT(Command.RequestState, 0));
            }
        }

        public void UpdateLightValue()
        {
            if (serialCOM.IsOpen)
            {
                for (int i = 1; i <= LightChannelCount; i++)
                {
                    WriteData(serialCOM.PortName, ConvertVIT(Command.RequestValue, i));
                }
            }
        }

        public void UpdateTemp()
        {
         //DIO pin 8번 이용해야 함
        }

        public int GetDimmingValue(int channel)
        {
            return dimmingValueArray[channel - 1];
        }

        public bool GetLightState(int channel)
        {
            return lightStateArray[channel - 1];
        }

        public int GetLightTemp(int channel)
        {
            return lightTempArray[channel - 1];
        }

    }
}
