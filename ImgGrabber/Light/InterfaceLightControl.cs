using System.Collections.Generic;

namespace ImgGrabber
{
    public delegate void DelLightReceive(object command);
    public delegate void DelLightConnect(bool bConnected);

    public interface ILightControl
    {
        FormMain Parent { get; set; }
        int LightChannelCount { get; set; }
        int ComPortNo { get; set; }
        int MaxValue { get; set; }
        List<int> InitValue { get; set; }

        bool CheckTemp { get; set; }
        bool IsOpen { get; set; }

        event DelLightReceive ReceiveEvent;
        event DelLightConnect ConnectEvent;

        bool InitLight();

        void ChangeValue(List<int> vs);
        void SetLightValue(int channel, int value);
        void LightAllOnOff(bool onOff);
        void LightOnOff(int channel, bool onOff);

        void UpdateTemp();
        void UpdateLightState();
        void UpdateLightValue();

        int GetLightTemp(int channel);
        bool GetLightState(int channel);
        int GetDimmingValue(int channel);

        void Dispose();
    }
}
