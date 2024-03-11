using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PalServerTools.Utils
{
    public static class MemoryUtil
    {
        // Import the Windows API function only if we are running on Windows.
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        public static bool ClearProcessWorkingSet(Process process)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    EmptyWorkingSet(process.Handle);
                    return true;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // 清除页缓存
                    RunCommand("echo 1 > /proc/sys/vm/drop_caches");
                    // 清除dentries和inodes
                    RunCommand("echo 2 > /proc/sys/vm/drop_caches");
                    // 清除页缓存、dentries和inodes
                    RunCommand("echo 3 > /proc/sys/vm/drop_caches");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not clear process working set. " + ex.Message);
            }
            return false;
        }

        private static void RunCommand(string command)
        {
            // 使用echo命令需要以root用户身份执行
            using (var process = new Process())
            {
                process.StartInfo.FileName = "bash";
                process.StartInfo.Arguments = "-c \"" + command + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();
            }
        }
    }
}
