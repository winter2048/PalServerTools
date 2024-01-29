using CronQuery.Mvc.Jobs;
using PalServerTools.Data;

namespace PalServerTools.Job
{
    public class BackupJob : IJob
    {
        private readonly PalProcessService _palProcessService;
        private readonly BackupService _backupService;

        public BackupJob(PalProcessService palProcessService, BackupService backupService)
        {
            _palProcessService = palProcessService;
            _backupService = backupService;
        }
        public async Task RunAsync()
        {
            if (_palProcessService.palServerState == Models.PalEnum.PalServerState.Running)
            {
                _backupService.Backup();
            }
        }
    }
}
