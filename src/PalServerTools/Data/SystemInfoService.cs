using PalServerTools.Utils;
using System.Diagnostics;
using System.Management;
using static PalServerTools.Models.PalEnum;

namespace PalServerTools.Data
{
    public class SystemInfoService
    {
        private PerformanceCounter cpuCounter;
        private PerformanceCounter memoryCounter;
        private ManagementObjectSearcher managementObject;

        public SystemInfo Info { get; set; }

        public SystemInfoService()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memoryCounter = new PerformanceCounter("Memory", "Available Bytes");
            managementObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            Info = new SystemInfo();
        }

        public void RefreshInfo()
        {
            Info.OSVersion = GetOSVersion();
            Info.CPUCount = GetCPUCount();
            Info.TotalMemory = GetTotalMemory();
            Info.CPUUsage = GetCPUUsage();
            Info.MemoryUsage = GetMemoryUsage();
        }

        public string GetOSVersion()
        {
            return Environment.OSVersion.VersionString;
        }

        public int GetCPUCount()
        {
            return Environment.ProcessorCount;
        }

        public long GetTotalMemory()
        {
            long ss = 0;
            foreach (ManagementObject mo in managementObject.Get())
            {
                ss = Convert.ToInt64(mo["TotalVisibleMemorySize"]);
                //Console.WriteLine("Computer Name : " + mo["csname"]);
                //Console.WriteLine("Windows Directory : " + mo["WindowsDirectory"]);
                //Console.WriteLine("Operating System: " + mo["Caption"]);
                //Console.WriteLine("Version: " + mo["Version"]);
                //Console.WriteLine("Manufacturer: " + mo["Manufacturer"]);
                //Console.WriteLine("Total Visible Memory: " + mo["TotalVisibleMemorySize"]);
                //Console.WriteLine("Free Physical Memory: " + mo["FreePhysicalMemory"]);
            }
            return ss / 1024;
        }

        public double GetCPUUsage()
        {
            return Math.Round(cpuCounter.NextValue(), 2);
        }

        public double GetMemoryUsage()
        {
            return Math.Round(100 - ((memoryCounter.NextValue() / 1024 / 1024 / GetTotalMemory()) * 100), 2);
        }
    }
}
