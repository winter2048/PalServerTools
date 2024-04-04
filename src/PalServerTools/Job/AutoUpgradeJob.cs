using CronQuery.Mvc.Jobs;
using Microsoft.Extensions.Logging;
using PalServerTools.Data;
using PalServerTools.Models;
using System.Diagnostics;

namespace PalServerTools.Job
{
    public class AutoUpgradeJob : IJob
    {
        private readonly PalProcessService _palProcessService;
        private readonly PalConfigService _configService;
        private readonly ILogger _logger;

        public AutoUpgradeJob(PalProcessService palProcessService, PalConfigService configService, ILogger logger)
        {
            _palProcessService = palProcessService;
            _configService = configService;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            try
            {
                await _palProcessService.CheckLatestVersion();
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message);
            }
            // 如果有新版本，则升级
            if (!_palProcessService.isLatestVersion && _palProcessService.palServerUpdateState != PalServerUpdateState.Updating)
            {
                _logger.LogInformation($"发现PalServer新版本 {_palProcessService.latestVersion}，开始自动更新...");
                await _palProcessService.Upgrade();
            }
        }
    }
}
