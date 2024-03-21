using Microsoft.Extensions.Logging;
using PalServerTools.Utils;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PalServerTools.Data
{
    public class SystemInfoService
    {
        [SupportedOSPlatform("Windows")]
        private readonly PerformanceCounter? cpuCounter;
        [SupportedOSPlatform("Windows")]
        private readonly PerformanceCounter? memoryCounter;
        [SupportedOSPlatform("Windows")]
        private readonly ManagementObjectSearcher? managementObject;

        private readonly ILogger _logger;

        public SystemInfo Info { get; set; }

        public SystemInfoService(ILogger logger)
        {
            _logger = logger;
            Info = new SystemInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                memoryCounter = new PerformanceCounter("Memory", "Available Bytes");
                managementObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            }

            // Initiate a refresh to populate the Info properties
            RefreshInfo();
        }

        public void RefreshInfo()
        {
            Info.OSVersion = GetOSVersion();
            Info.CPUCount = GetCPUCount();
            Info.CPUUsage = GetCPUUsage();
            Info.TotalMemory = GetTotalMemory();
            Info.MemoryUsage = GetMemoryUsage();
        }

        private string GetOSVersion()
        {
            return Environment.OSVersion.VersionString;
        }

        private int GetCPUCount()
        {
            return Environment.ProcessorCount;
        }

        private long GetTotalMemory()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string memInfo = File.ReadAllText("/proc/meminfo");
                    var match = System.Text.RegularExpressions.Regex.Match(memInfo, @"MemTotal:\s+(\d+) kB");
                    if (match.Success)
                    {
                        return long.Parse(match.Groups[1].Value);
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && managementObject != null)
                {
                    long totalVisibleMemorySize = 0;
                    foreach (ManagementObject mo in managementObject.Get())
                    {
                        totalVisibleMemorySize = Convert.ToInt64(mo["TotalVisibleMemorySize"]);
                    }
                    return totalVisibleMemorySize / 1024;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return 0;
        }

        private double GetCPUUsage()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    double cpuUsage = 0.0;
                    var firstMeasure = ReadCpuUsage();
                    Thread.Sleep(500); // 等待一段时间来计算利用率
                    var secondMeasure = ReadCpuUsage();
                    cpuUsage = ((secondMeasure.used - firstMeasure.used) * 100.0) / (secondMeasure.total - firstMeasure.total);
                    return cpuUsage;

                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && cpuCounter != null)
                {
                    return Math.Round(cpuCounter.NextValue(), 2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return 0;
        }

        private (double used, double total) ReadCpuUsage()
        {
            string cpuStats = File.ReadAllText("/proc/stat");
            var firstLine = cpuStats.Split('\n')[0]; // First line is cpu stats
            var val = firstLine.Split(' ').Skip(2).Select(v => double.Parse(v)).ToArray();
            double total = val.Sum();
            double idle = val[3];
            return (total - idle, total);
        }

        private double GetMemoryUsage()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string memInfo = File.ReadAllText("/proc/meminfo");
                    var matchTotal = System.Text.RegularExpressions.Regex.Match(memInfo, @"MemTotal:\s+(\d+) kB");
                    var matchFree = System.Text.RegularExpressions.Regex.Match(memInfo, @"MemFree:\s+(\d+) kB");
                    var matchBuffers = System.Text.RegularExpressions.Regex.Match(memInfo, @"Buffers:\s+(\d+) kB");
                    var matchCached = System.Text.RegularExpressions.Regex.Match(memInfo, @"Cached:\s+(\d+) kB");
                    if (matchTotal.Success && matchFree.Success && matchBuffers.Success && matchCached.Success)
                    {
                        long totalMemory = long.Parse(matchTotal.Groups[1].Value);
                        long freeMemory = long.Parse(matchFree.Groups[1].Value);
                        long buffers = long.Parse(matchBuffers.Groups[1].Value);
                        long cached = long.Parse(matchCached.Groups[1].Value);
                        long usedMemory = totalMemory - (freeMemory + buffers + cached);
                        return (usedMemory * 100.0) / totalMemory;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && memoryCounter != null)
                {
                    return Math.Round(100 - ((memoryCounter.NextValue() / 1024 / 1024 / Info.TotalMemory) * 100), 2);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return 0;
        }
    }
}
