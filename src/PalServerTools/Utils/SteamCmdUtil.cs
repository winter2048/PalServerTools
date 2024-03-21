using OneOf.Types;
using PalServerTools.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PalServerTools.Utils
{
    public class SteamCmdUtil
    {
        public static bool HasSteamCMD()
        {
            bool found = false;

            string steamCMDPath = AppUtil.ServiceProvider.GetRequiredService<PalConfigService>().ToolsConfig.SteamCMDPath;
            if (!string.IsNullOrWhiteSpace(steamCMDPath))
            {
                // 从配置的路径中查找 steamcmd.exe
                string steamCmdPath = Path.Combine(steamCMDPath, "steamcmd.exe");
                if (File.Exists(steamCmdPath))
                {
                    found = true;
                }
            }
            else
            {
                // 从系统环境变量中查找 steamcmd.exe
                var pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);

                if (pathVariable != null)
                {
                    // 拆分PATH环境变量为单独的目录路径
                    string[] paths = pathVariable.Split(Path.PathSeparator);

                    // 遍历每个目录路径
                    foreach (var path in paths)
                    {
                        // 构建steamcmd.exe的完整路径
                        string steamCmdPath = Path.Combine(path, "steamcmd.exe");

                        // 检查文件是否存在
                        if (File.Exists(steamCmdPath))
                        {
                            found = true;
                            break;
                        }
                    }
                }
            }

            return found;
        }

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

            // 设置steamcmd路径
            string steamCMDPath = AppUtil.ServiceProvider.GetRequiredService<PalConfigService>().ToolsConfig.SteamCMDPath;
            if (!string.IsNullOrWhiteSpace(steamCMDPath))
            {
                startInfo.WorkingDirectory = steamCMDPath;
            }

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

                        AppUtil.Logger.LogInformation("Output: " + result); // 打印输出结果
                    }

                    // 读取错误输出
                    using (StreamReader reader = process.StandardError)
                    {
                        string error = await reader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            msg = error;
                            AppUtil.Logger.LogError("Error: " + error); // 打印错误信息
                        }
                    }

                    process.WaitForExit(); // 等待进程结束

                    return new Tuple<bool, string>(process.ExitCode == 0, msg); // 进程返回0通常表示成功
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                AppUtil.Logger.LogError("Exception occurred: " + ex.Message);
                return new Tuple<bool, string>(false, msg);
            }
        }
    }
}
