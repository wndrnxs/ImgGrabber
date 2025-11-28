using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ImgGrabber
{
    /// <summary>
    /// 네트워크 드라이브 에러코드 Class
    /// </summary>
    public class ERROR_CODE
    {
        public const int NO_ERROR = 0;
        public const int ERROR_NO_NET_OR_BAD_SERVER = 53;
        public const int ERROR_BAD_USER_OR_PASSWORD = 1326;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_ALREADY_ASSIGNED = 85;
        public const int ERROR_BAD_DEV_TYPE = 66;
        public const int ERROR_BAD_DEVICE = 1200;
        public const int ERROR_BAD_NET_NAME = 67;
        public const int ERROR_BAD_PROFILE = 1206;
        public const int ERROR_BAD_PROVIDER = 1204;
        public const int ERROR_BUSY = 170;
        public const int ERROR_CANCELLED = 1223;
        public const int ERROR_CANNOT_OPEN_PROFILE = 1205;
        public const int ERROR_DEVICE_ALREADY_REMEMBERED = 1202;
        public const int ERROR_EXTENDED_ERROR = 1208;
        public const int ERROR_INVALID_PASSWORD = 86;
        public const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        public const int ERROR_INVALID_ADDRESS = 487;
        public const int ERROR_NETWORK_BUSY = 54;
        public const int ERROR_UNEXP_NET_ERR = 59;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_MULTIPLE_CONNECTION = 1219;
    }
    public class NetConnector
    {
        public NETRESOURCE NetResource = new NETRESOURCE();
        [DllImport("mpr.dll", CharSet = CharSet.Auto)]
        public static extern int WNetUseConnection(IntPtr hwndOwner, [MarshalAs(UnmanagedType.Struct)] ref NETRESOURCE lpNetResource,
                    string lpPassword, string lpUserID, uint dwFlags,
                    StringBuilder lpAccessName, ref int lpBufferSize, out uint lpResult);

        [DllImport("mpr.dll", EntryPoint = "WNetCancelConnection2", CharSet = CharSet.Auto)]
        public static extern int WNetCancelConnection2A(string lpName, int dwFlags, int fForce);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct NETRESOURCE
        {
            public uint dwScope;
            public uint dwType;
            public uint dwDisplayType;
            public uint dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }
        public int TryConnectNetwork(string remotePath, string userID, string pwd)
        {
            try
            {
                int capacity = 1028;
                uint flags = 0;
                StringBuilder sb = new StringBuilder(capacity);
                NetResource.dwType = 1; // 공유 디스크
                NetResource.lpLocalName = null;   // 로컬 드라이브 지정하지 않음
                NetResource.lpRemoteName = remotePath;
                NetResource.lpProvider = null;

                int result = WNetUseConnection(IntPtr.Zero, ref NetResource, pwd, userID, flags, sb, ref capacity, out uint resultFlags);

                return result;
            }
            catch (TimeoutException)
            {
                return -1;
            }

        }
        public void DisconnectNetwork()
        {
            WNetCancelConnection2A(NetResource.lpRemoteName, 1, 0);
        }

    }
}
