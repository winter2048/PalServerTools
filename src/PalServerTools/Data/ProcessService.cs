using AntDesign;
using PalServerTools.Utils;
using System;
using System.Diagnostics;
using System.Reactive.Joins;
using System.Text.RegularExpressions;
using static PalServerTools.Models.PalEnum;

namespace PalServerTools.Data
{
    public class PalProcessService
    {
        private readonly PalConfigService _configService;
        private readonly PalRconService _palRconService;
        private readonly SystemInfoService _systemInfoService;
        private string processName = "PalServer";

        public PalServerUpdateState palServerUpdateState = PalServerUpdateState.None;
        public PalServerState palServerState = PalServerState.Stopped;
        public bool isLatestVersion = true;
        public string latestVersion = "";
        public string currentVersion = "";

        public PalProcessService(PalConfigService configService, PalRconService palRconService, SystemInfoService systemInfoService)
        {
            _configService = configService;
            _palRconService = palRconService;
            _systemInfoService = systemInfoService;
        }

        // 启动进程
        public void StartProcess()
        {
            try
            {
                Process.Start(Path.Combine(_configService.ToolsConfig.PalServerPath, "PalServer.exe"), _configService.ToolsConfig.RunArguments);
                palServerState = PalServerState.Running;
                Console.WriteLine("启动进程 " + processName + ".exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动进程失败: " + ex.Message);
            }
        }

        // 结束进程
        public void CloseProcess()
        {
            try
            {
                Process[] processes = Process.GetProcesses().Where(p => p.ProcessName == "PalServer-Win64-Test-Cmd" || p.ProcessName == "PalServer-Win64-Test" || p.ProcessName == "PalServer").ToArray();
                foreach (Process process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                    palServerState = PalServerState.Stopped;
                    Console.WriteLine("进程 " + processName + ".exe 已关闭");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("启动进程失败: " + ex.Message);
            }
        }    
        
        // 检查进程是否存在
        public bool IsProcessRunning()
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        public void CheckProcessStatus()
        {
            bool isProcessRunning = IsProcessRunning();
            palServerState = isProcessRunning ? PalServerState.Running : PalServerState.Stopped;
        }

        public async Task CheckLatestVersion()
        {
            if ((string.IsNullOrWhiteSpace(currentVersion) || !isLatestVersion) && palServerState == PalServerState.Running)
            {
                var info = await _palRconService.Info();
                if (!string.IsNullOrWhiteSpace(info))
                {
                    Match match = Regex.Match(info, @"v\d+\.\d+\.\d+\.\d+");
                    if (match.Success)
                    {
                        currentVersion = match.Value;
                    }
                }
            }

            if (isLatestVersion && palServerState == PalServerState.Running)
            {
                var rssItem = RssUtil.ReadRss("https://store.steampowered.com/feeds/news/app/1623730/?cc=CN&l=schinese&snr=1_2108_9__2107");
                foreach (var item in rssItem)
                {
                    var rssTitle = item.Title.Text;
                    if (rssTitle != null)
                    {
                        Match match = Regex.Match(rssTitle, @"v\d+\.\d+\.\d+\.\d+");
                        if (match.Success)
                        {
                            latestVersion = match.Value;
                            if (latestVersion == currentVersion)
                            {
                                isLatestVersion = true;
                            }
                            else
                            {
                                isLatestVersion = false;
                            }
                            break;
                        }
                    }
                }
            }
        }

        public async Task Upgrade()
        {
            palServerUpdateState = PalServerUpdateState.Updating;
            try
            {
                if (palServerState == PalServerState.Running)
                {
                    CloseProcess();
                }

                var res = await SteamCmdUtil.AppUpdate(2394010);
                if (!res.Item1)
                {
                    throw new Exception(res.Item2);
                }
                palServerUpdateState = PalServerUpdateState.Success;
                StartProcess();
            }
            catch (Exception)
            {
                palServerUpdateState = PalServerUpdateState.Failed;
                throw;
            }
        }
    
        public void ClearProcessMemory()
        {
            if (_systemInfoService.Info.MemoryUsage >= 80)
            {
                MemoryUtil.ClearProcessWorkingSet(processName);
            }
        }
    }
}
