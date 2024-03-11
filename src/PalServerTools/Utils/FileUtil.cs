namespace PalServerTools.Utils
{
    public static class FileUtil
    {
        private static Dictionary<string, FileSystemWatcher> watchFilePaths = new Dictionary<string, FileSystemWatcher>();

        public static bool IsFileReadable(string filepath)
        {
            try
            {
                // 尝试以读取模式打开文件
                using (FileStream stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // 如果没有异常，那么文件是可读的
                    return true;
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException || ex is UnauthorizedAccessException)
            {
                // 如果遭遇上述异常之一，文件不可读
                // 文件不存在，I/O错误，或无读权限
                return false;
            }
        }

        public static void CleanWatchFileAll()
        {
            foreach (var item in watchFilePaths)
            {
                try
                {
                    item.Value.Dispose();
                    watchFilePaths.Remove(item.Key);
                }
                catch{}
            }
        }

        public static void WatchFile(string filePath, Action<object, FileSystemEventArgs> OnChanged)
        {
            try
            {
                if (!watchFilePaths.ContainsKey(filePath))
                {
                    // 创建一个新的FileSystemWatcher实例
                    FileSystemWatcher watcher = new FileSystemWatcher();

                    // 设置监听的文件路径
                    watcher.Path = Path.GetDirectoryName(filePath);

                    // 设置要监听的文件名
                    watcher.Filter = Path.GetFileName(filePath);

                    // 设置要监听的事件类型
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                    // 添加事件处理程序
                    watcher.Changed += (e, k) => { Console.WriteLine("File Changed: " + filePath); OnChanged(e, k); };
                    watcher.Created += (e, k) => { Console.WriteLine("File Changed: " + filePath); OnChanged(e, k); };
                    watcher.Deleted += (e, k) => { Console.WriteLine("File Changed: " + filePath); OnChanged(e, k); };
                    watcher.Renamed += (e, k) => { Console.WriteLine("File Changed: " + filePath); OnChanged(e, k); };

                    // 开始监听
                    watcher.EnableRaisingEvents = true;
                    watchFilePaths.Add(filePath, watcher);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FileUtil.WatchFile: " + ex.Message);
            }
        }

        private static void WatchFile(string dirPath, string fileName, Action<object, FileSystemEventArgs> OnChanged)
        {
            try
            {
                if (!watchFilePaths.ContainsKey(dirPath + fileName))
                {
                    // 创建一个新的FileSystemWatcher实例
                    FileSystemWatcher watcher = new FileSystemWatcher();

                    // 设置监听的文件路径
                    watcher.Path = dirPath;

                    // 设置要监听的文件名
                    watcher.Filter = fileName;

                    // 设置要监听的事件类型
                    watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                    // 添加事件处理程序
                    watcher.Changed += (e, k) => { OnChanged(e, k); };
                    watcher.Created += (e, k) => { OnChanged(e, k); };
                    watcher.Deleted += (e, k) => { OnChanged(e, k); };
                    watcher.Renamed += (e, k) => { OnChanged(e, k); };

                    // 开始监听
                    watcher.EnableRaisingEvents = true;
                    watchFilePaths.Add(dirPath + fileName, watcher);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("FileUtil.WatchFile: " + ex.Message);
            }
        }


        public static void WatchFile(string dirPath, string fileName, Action<object, FileSystemEventArgs> OnChanged, bool allDir = false)
        {
            // 监听基础目录
            WatchFile(dirPath, fileName, OnChanged);
            if (allDir)
            {
                // 监听所有子目录
                foreach (var directory in Directory.EnumerateDirectories(dirPath, "*", SearchOption.AllDirectories))
                {
                    WatchFile(directory, fileName, OnChanged);
                }
            }
            
        }
    }
}
