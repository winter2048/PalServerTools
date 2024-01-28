using CronQuery.Mvc.Jobs;
using PalServerTools.Data;

namespace PalServerTools.Job
{
    public class BackupJob : IJob
    {
        private readonly PalProcessService _palProcessService;
        private readonly ConfigService _configService;

        public BackupJob(PalProcessService palProcessService, ConfigService configService)
        {
            _palProcessService = palProcessService;
            _configService = configService;
        }
        public async Task RunAsync()
        {
            if (_palProcessService.palServerState == Models.PalEnum.PalServerState.Running)
            {
               // _configService.ToolsConfig.BackupPath
            }
            Console.WriteLine("11111");
        }
    }
}
