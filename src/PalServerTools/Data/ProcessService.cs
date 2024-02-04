using System;
using System.Diagnostics;
using static PalServerTools.Models.PalEnum;

namespace PalServerTools.Data
{
    public class PalProcessService
    {
        private readonly PalConfigService _configService;
        private string processName = "PalServer";

        public PalServerState palServerState;

        public PalProcessService(PalConfigService configService) {
            _configService = configService;
        }

        // 启动进程
        public void StartProcess()
        {
            try
            {
                Process.Start(Path.Combine(_configService.ToolsConfig.PalServerPath, "PalServer.exe"), _configService.ToolsConfig.RunArguments);
                palServerState = PalServerState.Running;
                Console.WriteLine("启动进程 " + processName + ".exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动进程失败: " + ex.Message);
            }
        }

        // 结束进程
        public void CloseProcess()
        {
            try
            {
                Process[] processes = Process.GetProcesses().Where(p => p.ProcessName == "PalServer-Win64-Test-Cmd" || p.ProcessName == "PalServer-Win64-Test" || p.ProcessName == "PalServer").ToArray();
                foreach (Process process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                    palServerState = PalServerState.Stopped;
                    Console.WriteLine("进程 " + processName + ".exe 已关闭");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动进程失败: " + ex.Message);
            }
        }
    }
}
