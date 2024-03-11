using System.Diagnostics;
using System.Management;

namespace PalServerTools.Utils
{
    public static class ProcessUtil
    {
        /// <summary>
        /// 根据文件路径获取进程
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Process[] GetProcessesByPath(string filePath)
        {
            List<Process> processes = new List<Process>();

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (process.MainModule.FileName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        processes.Add(process);
                    }
                }
                catch { }
            }
            return processes.ToArray();
        }

        /// <summary>
        /// 获取进程的命令行参数
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public static string? GetCommandLine(Process process)
        {
            string commandLine = "";

            using (ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (ManagementObject mo in mos.Get())
                {
                    commandLine = mo["CommandLine"].ToString();
                    break;
                }
            }

            return commandLine;
        }
    }
}
