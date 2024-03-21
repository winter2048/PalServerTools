using System;

namespace PalServerTools.Utils
{
    public class AppUtil
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static ILogger Logger => ServiceProvider.GetRequiredService<ILogger>();

        public static string Env { get; set; } = "";

        /// <summary>
        /// 处理版本差异
        /// </summary>
        public static void HistoryProcessor()
        {
            // 兼容历史版本：迁移旧配置文件 appsettings*.json => appsetting*.json
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = Directory.GetFiles(directory, "appsettings*.json");
            foreach (string file in files)
            {
                string newFilePath = file.Replace("appsettings", "appsetting");

                File.Move(file, newFilePath, true);
                Console.WriteLine($"迁移旧配置文件: {file} -> {newFilePath}");
            }
        }
    }
}
