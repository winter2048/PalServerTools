using CronQuery.Mvc.Jobs;
using PalServerTools.Data;
using static PalServerTools.Models.PalEnum;
using System.Diagnostics;

namespace PalServerTools.Job
{
    public class AutoUpgradeJob : IJob
    {
        private readonly PalProcessService _palProcessService;
        private readonly PalConfigService _configService;

        public AutoUpgradeJob(PalProcessService palProcessService, PalConfigService configService)
        {
            _palProcessService = palProcessService;
            _configService = configService;
        }

        public async Task RunAsync()
        {
            await _palProcessService.CheckLatestVersion();
            // 如果有新版本，则升级
            if (!_palProcessService.isLatestVersion && _palProcessService.palServerUpdateState != PalServerUpdateState.Updating)
            {
               await _palProcessService.Upgrade();
            }
        }
    }
}
