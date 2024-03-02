namespace PalServerTools.Models
{
    public class ToolsConfigModel
    {
        public string ToolsPassword { get; set; } = "";
        public string PalServerPath { get; set; } = "";

        public string RunArguments { get; set; } = "";

        public bool AutoRestart { get; set; }

        public bool AutoBackup { get; set; }

        public string BackupCron { get; set; } = "";

        public string BackupPath { get; set; } = "";

        public int BackupClearType { get; set; }

        public int BackupCount { get; set; }

        public bool AutoUpgrade { get; set; }

        public bool MemoryClear { get; set; }
    }
}
