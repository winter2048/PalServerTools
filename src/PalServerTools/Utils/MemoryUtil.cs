using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PalServerTools.Utils
{
    public static class MemoryUtil
    {
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        public static bool ClearProcessWorkingSet(string processName)
        {
            try
            {
                var procs = Process.GetProcessesByName(processName);
                foreach (var proc in procs)
                {
                    EmptyWorkingSet(proc.Handle);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not clear process working set. " + ex.Message);
                return false;
            }
        }
    }
}
