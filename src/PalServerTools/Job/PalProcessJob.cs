using CronQuery.Mvc.Jobs;
using PalServerTools.Data;
using static PalServerTools.Models.PalEnum;
using System.Diagnostics;

namespace PalServerTools.Job
{
    public class PalProcessJob : IJob
    {
        private readonly PalProcessService _palProcessService;
        private readonly PalConfigService _configService;
        private string processName = "PalServer";

        public PalProcessJob(PalProcessService palProcessService, PalConfigService configService)
        {
            _palProcessService = palProcessService;
            _configService = configService;
        }

        public async Task RunAsync()
        {
            CheckProcessStatus();
        }

        // 检查进程状态的方法
        public void CheckProcessStatus()
        {
            bool isProcessRunning = IsProcessRunning();
            _palProcessService.palServerState = isProcessRunning ? PalServerState.Running : PalServerState.Stopped;
            // 如果进程不存在，则启动进程
            if (!isProcessRunning && _configService.ToolsConfig.AutoRestart)
            {
                _palProcessService.StartProcess();
            }
        }

        // 检查进程是否存在
        public bool IsProcessRunning()
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }
    }
}
