using OneOf.Types;
using System.Diagnostics;
using System.Text;

namespace PalServerTools.Utils
{
    public class SteamCmdUtil
    {
        public static async Task<Tuple<bool, string>> AppUpdate(int appId)
        {
            string msg = "";
            // 设置SteamCMD的执行命令行
            string command = "steamcmd +login anonymous +app_update "+ appId + " validate +quit";

            // 创建过程启动信息
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",  // cmd.exe用于执行命令
                Arguments = "/c " + command, // "/c"参数表示执行完命令后关闭命令窗口
                UseShellExecute = false,
                RedirectStandardOutput = true, // 重定向输出
                RedirectStandardError = true, // 重定向错误输出
                CreateNoWindow = true, // 不创建新窗口
                StandardOutputEncoding = Encoding.UTF8, // 设置标准输出的编码为UTF-8
                StandardErrorEncoding = Encoding.UTF8 // 设置错误输出的编码为UTF-8
            };

            try
            {
                // 启动进程
                using (Process process = Process.Start(startInfo))
                {
                    // 读取输出
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = await reader.ReadToEndAsync();
                        msg = result;
                        Console.WriteLine("Output: " + result); // 打印输出结果
                    }

                    // 读取错误输出
                    using (StreamReader reader = process.StandardError)
                    {
                        string error = await reader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            msg = error;
                            Console.WriteLine("Error: " + error); // 打印错误信息
                        }
                    }

                    process.WaitForExit(); // 等待进程结束

                    return new Tuple<bool, string>(process.ExitCode == 0, msg); // 进程返回0通常表示成功
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                Console.WriteLine("Exception occurred: " + ex.Message);
                return new Tuple<bool, string>(false, msg);
            }
        }
    }
}
