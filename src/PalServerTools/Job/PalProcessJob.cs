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

        public PalProcessJob(PalProcessService palProcessService, PalConfigService configService)
        {
            _palProcessService = palProcessService;
            _configService = configService;
        }

        public async Task RunAsync()
        {
            _palProcessService.CheckProcessStatus();
            // 如果进程不存在，则启动进程
            if (_palProcessService.palServerState == PalServerState.Stopped && _configService.ToolsConfig.AutoRestart && _palProcessService.palServerUpdateState != PalServerUpdateState.Updating)
            {
                _palProcessService.StartProcess();
            }

            await _palProcessService.CheckLatestVersion();
            // 如果有新版本，则升级
            if (!_palProcessService.isLatestVersion && _configService.ToolsConfig.AutoRestart && _palProcessService.palServerUpdateState != PalServerUpdateState.Updating)
            {
               await _palProcessService.Upgrade();
            }

            // 开启内存优化
            if (_palProcessService.palServerState == PalServerState.Running && _configService.ToolsConfig.MemoryClear)
            {
                _palProcessService.ClearProcessMemory();
            }
        }
    }
}
