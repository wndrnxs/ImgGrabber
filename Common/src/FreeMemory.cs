using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Common
{
    //Declaration of structures
    //SYSTEM_CACHE_INFORMATION
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SYSTEM_CACHE_INFORMATION
    {
        public uint CurrentSize;
        public uint PeakSize;
        public uint PageFaultCount;
        public uint MinimumWorkingSet;
        public uint MaximumWorkingSet;
        public uint Unused1;
        public uint Unused2;
        public uint Unused3;
        public uint Unused4;
    }

    //SYSTEM_CACHE_INFORMATION_64_BIT
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SYSTEM_CACHE_INFORMATION_64_BIT
    {
        public long CurrentSize;
        public long PeakSize;
        public long PageFaultCount;
        public long MinimumWorkingSet;
        public long MaximumWorkingSet;
        public long Unused1;
        public long Unused2;
        public long Unused3;
        public long Unused4;
    }

    //TokPriv1Luid
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TokPriv1Luid
    {
        public int Count;
        public long Luid;
        public int Attr;
    }

    public class FreeMemory
    {

        //Declaration of constants
        private const int SE_PRIVILEGE_ENABLED = 2;
        private const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";
        private const string SE_PROFILE_SINGLE_PROCESS_NAME = "SeProfileSingleProcessPrivilege";
        private const int SystemFileCacheInformation = 0x15;
        private const int SystemMemoryListInformation = 0x50;
        private const int MemoryPurgeStandbyList = 4;


        //Import of DLL's (API) and the necessary functions 
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("ntdll.dll")]
        public static extern uint NtSetSystemInformation(int InfoClass, IntPtr Info, int Length);

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        //Function to clear working set of all processes
        public static void EmptyWorkingSetFunction()
        {
            //Cycle through all processes
            foreach (Process p in Process.GetProcesses())
            {
                //Try to empty the working set of the process, if succesfull add to successProcesses, if failed add to failProcesses with error message
                try
                {
                    EmptyWorkingSet(p.Handle);
                }
                catch
                {
                    continue;
                }
            }
        }

        //Function to check if OS is 64-bit or not, returns boolean
        public static bool Is64BitMode()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
        }



        //Function used to clear file system cache, returns boolean
        public static void ClearFileSystemCache(bool ClearStandbyCache)
        {
            try
            {
                uint num1 = 0, num2 = 0;
                //Check if privilege can be increased
                if (SetIncreasePrivilege(SE_INCREASE_QUOTA_NAME))
                {
                    int SystemInfoLength;
                    GCHandle gcHandle;
                    //First check which version is running, then fill structure with cache information. Throw error is cache information cannot be read.
                    if (!Is64BitMode())
                    {
                        SYSTEM_CACHE_INFORMATION cacheInformation = new SYSTEM_CACHE_INFORMATION
                        {
                            MinimumWorkingSet = uint.MaxValue,
                            MaximumWorkingSet = uint.MaxValue
                        };
                        SystemInfoLength = Marshal.SizeOf(cacheInformation);
                        gcHandle = GCHandle.Alloc(cacheInformation, GCHandleType.Pinned);
                        num1 = NtSetSystemInformation(SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                        gcHandle.Free();
                    }
                    else
                    {
                        SYSTEM_CACHE_INFORMATION_64_BIT information64Bit = new SYSTEM_CACHE_INFORMATION_64_BIT
                        {
                            MinimumWorkingSet = -1L,
                            MaximumWorkingSet = -1L
                        };
                        SystemInfoLength = Marshal.SizeOf(information64Bit);
                        gcHandle = GCHandle.Alloc(information64Bit, GCHandleType.Pinned);
                        num1 = NtSetSystemInformation(SystemFileCacheInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                        gcHandle.Free();
                    }

                    if (num1 != 0)
                    {
                        throw new Exception("NtSetSystemInformation(SYSTEMCACHEINFORMATION) error: ", new Win32Exception(Marshal.GetLastWin32Error()));
                    }
                }

                //If passes paramater is 'true' and the privilege can be increased, then clear standby lists through MemoryPurgeStandbyList
                if (ClearStandbyCache && SetIncreasePrivilege(SE_PROFILE_SINGLE_PROCESS_NAME))
                {
                    int SystemInfoLength = Marshal.SizeOf(MemoryPurgeStandbyList);
                    GCHandle gcHandle = GCHandle.Alloc(MemoryPurgeStandbyList, GCHandleType.Pinned);
                    num2 = NtSetSystemInformation(SystemMemoryListInformation, gcHandle.AddrOfPinnedObject(), SystemInfoLength);
                    gcHandle.Free();

                }

                if (num2 != 0)
                {
                    throw new Exception("NtSetSystemInformation(SYSTEMMEMORYLISTINFORMATION) error: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }
            }
            catch
            {

            }
        }

        //Function to increase Privilege, returns boolean
        private static bool SetIncreasePrivilege(string privilegeName)
        {
            using (WindowsIdentity current = WindowsIdentity.GetCurrent(TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges))
            {
                TokPriv1Luid newst;
                newst.Count = 1;
                newst.Luid = 0L;
                newst.Attr = SE_PRIVILEGE_ENABLED;

                //Retrieves the LUID used on a specified system to locally represent the specified privilege name
                if (!LookupPrivilegeValue(null, privilegeName, ref newst.Luid))
                {
                    throw new Exception("Error in LookupPrivilegeValue: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }

                //Enables or disables privileges in a specified access token
                int num = AdjustTokenPrivileges(current.Token, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero) ? 1 : 0;
                if (num == 0)
                {
                    throw new Exception("Error in AdjustTokenPrivileges: ", new Win32Exception(Marshal.GetLastWin32Error()));
                }

                return num != 0;
            }
        }
    }
}


