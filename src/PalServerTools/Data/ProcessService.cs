using AntDesign;
using PalServerTools.Models;
using PalServerTools.Utils;
using System;
using System.Diagnostics;
using System.Reactive.Joins;
using System.Text.RegularExpressions;

namespace PalServerTools.Data
{
    public class PalProcessService
    {
        private readonly PalRconService _palRconService;
        private readonly SystemInfoService _systemInfoService;
        private readonly IServiceProvider _serviceProvider;
        private readonly string processName = "PalServer";
        private PalConfigService _configService => _serviceProvider.GetRequiredService<PalConfigService>();
        private string _palServerPath => Path.Combine(_configService.ToolsConfig.PalServerPath, "PalServer.exe");

        public PalServerUpdateState palServerUpdateState = PalServerUpdateState.None;
        public PalServerState palServerState = PalServerState.Stopped;
        public bool isLatestVersion = true;
        public string latestVersion = "";
        public string currentVersion = "";

        public PalProcessService(PalRconService palRconService, SystemInfoService systemInfoService, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _palRconService = palRconService;
            _systemInfoService = systemInfoService;
        }

        // 启动进程
        public async Task StartProcess()
        {
            await Task.Run(() =>
            {
                try
                {
                    string runArguments = _configService.ToolsConfig.RunArguments;
                    if (_configService.PalConfig.RCONEnabled)
                    {
                        if (!runArguments.Contains("-rconport"))
                        {
                            runArguments += $" -rconport {_configService.PalConfig.RCONPort}";
                        }
                        else if (runArguments.GetArgumentValue("-rconport") != _configService.PalConfig.RCONPort.ToString())
                        {
                            throw new Exception($"启动参数-rconport {runArguments.GetArgumentValue("-rconport")}与服务器配置中的RCON端口号({_configService.PalConfig.RCONPort})不一致！");
                        }
                    }
                    Process.Start(_palServerPath, runArguments);
                    palServerState = PalServerState.Running;
                    Console.WriteLine("启动进程 " + processName + ".exe");
                }
                catch (Exception ex)
                {
                    throw new Exception("启动进程失败: " + ex.Message);
                }
            });
        }

        // 结束进程
        public async Task CloseProcess()
        {
            await Task.Run(() =>
            {
                try
                {
                    string palServerPath = Path.Combine(_configService.ToolsConfig.PalServerPath, "PalServer.exe");
                    string PalServerWin64TestPath = Path.Combine(_configService.ToolsConfig.PalServerPath, @"Pal\Binaries\Win64", "PalServer-Win64-Test.exe");
                    string PalServerWin64TestCmdPath = Path.Combine(_configService.ToolsConfig.PalServerPath, @"Pal\Binaries\Win64", "PalServer-Win64-Test-Cmd.exe");
                    var processes = ProcessUtil.GetProcessesByPath(palServerPath)
                        .Concat(ProcessUtil.GetProcessesByPath(PalServerWin64TestPath))
                        .Concat(ProcessUtil.GetProcessesByPath(PalServerWin64TestCmdPath));

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
            });
        }

        // 检查进程是否存在
        public bool IsProcessRunning()
        {
            Process[] processes = ProcessUtil.GetProcessesByPath(_palServerPath);
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
                var rssItem = await RssUtil.ReadRss("https://store.steampowered.com/feeds/news/app/1623730/?cc=CN&l=schinese&snr=1_2108_9__2107");
                foreach (var item in rssItem)
                {
                    var rssTitle = item.Title.Text;
                    if (rssTitle != null)
                    {
                        Match match = Regex.Match(rssTitle, @"v\d+\.\d+\.\d+\.\d+");
                        if (match.Success)
                        {
                            latestVersion = match.Value;
                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(latestVersion) && !string.IsNullOrWhiteSpace(currentVersion))
            {
                isLatestVersion = (latestVersion == currentVersion);
            }
        }

        public async Task Upgrade()
        {
            palServerUpdateState = PalServerUpdateState.Updating;
            try
            {
                if (palServerState == PalServerState.Running)
                {
                   await CloseProcess();
                }

                var res = await SteamCmdUtil.AppUpdate(2394010);
                if (!res.Item1)
                {
                    throw new Exception(res.Item2);
                }
                palServerUpdateState = PalServerUpdateState.Success;
                await StartProcess();
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
                foreach (var item in ProcessUtil.GetProcessesByPath(_palServerPath))
                {
                    MemoryUtil.ClearProcessWorkingSet(item);
                }
            }
        }
    }
}
