using PalServerTools.Models;
using System.IO.Compression;

namespace PalServerTools.Data
{
    public class BackupService
    {
        private readonly ConfigService _configService;

        public BackupService(ConfigService configService)
        {
            _configService = configService;
        }

        public List<BackupModel> GetBackupList()
        {
            List<BackupModel> backupList = new List<BackupModel>();
            foreach (var file in Directory.GetFiles(_configService.ToolsConfig.BackupPath, "backup*.zip"))
            {
                var fileInfo = new FileInfo(file);
                backupList.Add(new BackupModel()
                {
                    Name = fileInfo.Name,
                    CreateTime = fileInfo.CreationTime,
                    Size = fileInfo.Length / 1024
                });
            }
            return backupList.OrderByDescending(x => x.CreateTime).ToList();
        }

        public void Backup()
        {
            string sourceFolderPath = Path.Combine(_configService.ToolsConfig.PalServerPath, @"Pal\Saved");
            string backupFolderPath = _configService.ToolsConfig.BackupPath;
            string zipFilePath = Path.Combine(backupFolderPath, $"backup{DateTime.Now.ToString("yyyyMMddHHmmss")}.zip");

            // 创建一个临时文件夹来存放压缩文件中的文件
            string tempFolderPath = Path.Combine(backupFolderPath, "temp");
            Directory.CreateDirectory(tempFolderPath);

            try
            {
                // 压缩Config文件夹
                string configFolderPath = Path.Combine(sourceFolderPath, "Config");
                string tempConfigFolderPath = Path.Combine(tempFolderPath, "Config");
                Directory.CreateDirectory(tempConfigFolderPath);
                CopyDirectory(configFolderPath, tempConfigFolderPath);

                // 压缩SaveGames文件夹
                string saveGamesFolderPath = Path.Combine(sourceFolderPath, "SaveGames");
                string tempSaveGamesFolderPath = Path.Combine(tempFolderPath, "SaveGames");
                Directory.CreateDirectory(tempSaveGamesFolderPath);
                CopyDirectory(saveGamesFolderPath, tempSaveGamesFolderPath);

                // 压缩临时文件夹
                ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);

                // 删除临时文件夹
                Directory.Delete(tempFolderPath, true);

                // 移动压缩文件到目标位置
                //File.Move(zipFilePath, Path.Combine(backupFolderPath, "backup.zip"));

                Console.WriteLine("压缩并移动成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误：" + ex.Message);
            }
        }


        public void DelBackup(string name)
        {
            var filePath = Path.Combine(_configService.ToolsConfig.BackupPath, name);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public Stream GetBackupStream(string name)
        {
            var filePath = Path.Combine(_configService.ToolsConfig.BackupPath, name);
            if (File.Exists(filePath))
            {
               return  File.OpenRead(filePath);
            }
            return null;
        }

        private void CopyDirectory(string sourceDir, string targetDir)
        { 
            // 接着递归复制源文件夹中的所有子文件夹到目标文件夹
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                if (!Directory.Exists(targetSubDir)) {
                    Directory.CreateDirectory(targetSubDir);
                }
                CopyDirectory(subDir, targetSubDir);
            }

            // 首先复制源文件夹中的所有文件到目标文件夹
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, true);
            }
        }
    }
}
