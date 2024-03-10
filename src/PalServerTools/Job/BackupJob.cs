using CronQuery.Mvc.Jobs;
using PalServerTools.Data;
using System.Linq;

namespace PalServerTools.Job
{
    public class BackupJob : IJob
    {
        private readonly PalProcessService _palProcessService;
        private readonly BackupService _backupService;
        private readonly PalConfigService _configService;

        public BackupJob(PalProcessService palProcessService, BackupService backupService, PalConfigService configService)
        {
            _palProcessService = palProcessService;
            _backupService = backupService;
            _configService = configService;
        }
        public async Task RunAsync()
        {
            if (_palProcessService.palServerState == Models.PalServerState.Running)
            {
               await _backupService.Backup();
            }
            if (_configService.ToolsConfig.BackupClearType == 1)
            {
                var delList = _backupService.GetBackupList().Skip(_configService.ToolsConfig.BackupCount).ToList();
                foreach (var item in delList)
                {
                    _backupService.DelBackup(item.Name);
                }
            }
        }
    }
}
