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
        private readonly SystemInfoService _systemInfoService;

        public PalProcessJob(PalProcessService palProcessService, PalConfigService configService, SystemInfoService systemInfoService)
        {
            _palProcessService = palProcessService;
            _configService = configService;
            _systemInfoService = systemInfoService;
        }

        public async Task RunAsync()
        {
            _systemInfoService.RefreshInfo();
            _palProcessService.CheckProcessStatus();
            // 如果进程不存在，则启动进程
            if (_palProcessService.palServerState == PalServerState.Stopped && _configService.ToolsConfig.AutoRestart && _palProcessService.palServerUpdateState != PalServerUpdateState.Updating)
            {
               await  _palProcessService.StartProcess();
            }

            // 开启内存优化
            if (_palProcessService.palServerState == PalServerState.Running && _configService.ToolsConfig.MemoryClear)
            {
                _palProcessService.ClearProcessMemory();
            }
        }
    }
}
