using PalServerTools.Models;
using System.IO.Compression;

namespace PalServerTools.Data
{
    public class BackupService
    {
        private readonly PalConfigService _configService;

        public BackupService(PalConfigService configService)
        {
            _configService = configService;
        }

        public List<BackupModel> GetBackupList()
        {
            List<BackupModel> backupList = new List<BackupModel>();
            if (Directory.Exists(_configService.ToolsConfig.BackupPath))
            {
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
            }
            return backupList.OrderByDescending(x => x.CreateTime).ToList();
        }

        public async Task Backup()
        {
           await Task.Run(() => {
                string sourceFolderPath = Path.Combine(_configService.ToolsConfig.PalServerPath, @"Pal\Saved");
                string backupFolderPath = _configService.ToolsConfig.BackupPath;

                if (!Directory.Exists(sourceFolderPath))
                {
                    throw new Exception("存档不存在！");
                }

                if (!Directory.Exists(backupFolderPath))
                {
                    throw new Exception("未配置备份路径！");
                }

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
            });
        }


        public void DelBackup(string name)
        {
            var filePath = Path.Combine(_configService.ToolsConfig.BackupPath, name);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                throw new Exception("文件不存在！");
            }
        }

        public async Task RestoreBackup(string backupName)
        {
            await Task.Run(() => {
                string sourceBackupPath = Path.Combine(_configService.ToolsConfig.BackupPath, backupName);
                string palServerSavePath = Path.Combine(_configService.ToolsConfig.PalServerPath, @"Pal\Saved");
                string saveGamesPath = Path.Combine(_configService.ToolsConfig.PalServerPath, @"Pal\Saved\SaveGames");

                string tempZipFolderPath = Path.Combine(palServerSavePath, "backup");

                if (!File.Exists(sourceBackupPath))
                {
                    throw new Exception("备份文件不存在！");
                }

                if (!Directory.Exists(palServerSavePath))
                {
                    throw new Exception("服务器存档路径不存在！");
                }

                // 删除现有的服务器存档文件夹
                if (Directory.Exists(saveGamesPath))
                {
                    Directory.Delete(saveGamesPath, true);
                }

                // 将备份文件解压到临时目录
                Directory.CreateDirectory(tempZipFolderPath);
                ZipFile.ExtractToDirectory(sourceBackupPath, tempZipFolderPath);
                // 还原存档
                CopyDirectory(Path.Combine(tempZipFolderPath, "SaveGames"), saveGamesPath);
                // 删除临时文件夹
                Directory.Delete(tempZipFolderPath, true);

                Console.WriteLine("存档恢复成功！");
            });
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
