using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PalServerTools.Data;
using PalServerTools.Models;
using Python.Included;
using Python.Runtime;
using System.Diagnostics;

namespace PalServerTools.Utils
{
    public static class PalSavUtil
    {
        private static readonly object pythonEngineLock = new object();
        public static InstallState installState = InstallState.None;

        public static async Task Init()
        {
            if (Installer.IsPythonInstalled() && Installer.IsPipInstalled())
            {
                try
                {
                    await Installer.SetupPython(true);
                    _ = Task.Run(async () => {
                       await Installer.TryInstallPip(true);
                       await Installer.PipInstallModule("palworld-save-tools", force: true);
                    });
                    installState = InstallState.Installed;
                }
                catch (Exception ex)
                {
                    AppUtil.Logger.LogError("SAV解析器palworld-save-tools初始化失败：" + ex.Message);
                }
            }
            else
            {
                _ = Install();
            }
        }

        public static async Task Install()
        {
            installState = InstallState.Installing;
            try
            {
                await Task.Run(async () =>
                {
                    if (!Installer.IsPythonInstalled())
                    {
                        await Console.Out.WriteLineAsync("安装Python....");
                        await Installer.SetupPython(true);
                        await Console.Out.WriteLineAsync("安装Python成功");
                    }
                    else
                    {

                    }
                    if (!Installer.IsPipInstalled())
                    {
                        await Console.Out.WriteLineAsync("安装Python.pip....");
                        await Installer.TryInstallPip(true);
                        await Console.Out.WriteLineAsync("安装Python.pip成功");
                    }
                    else
                    {

                    }
                    if (!Installer.IsModuleInstalled("palworld-save-tools"))
                    {
                        await Console.Out.WriteLineAsync("安装Python.module palworld-save-tools....");
                        await Installer.PipInstallModule("palworld-save-tools", force: true);
                        await Console.Out.WriteLineAsync("安装Python.module palworld-save-tools成功");
                    }
                    else
                    {
                        await Installer.PipInstallModule("palworld-save-tools", force: true);
                    }
                });
                installState = InstallState.Installed;
                // 重启应用
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "./update.ps1 -Restart",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = AppContext.BaseDirectory
                };

                Process process = Process.Start(psi);
                //AppUtil.ServiceProvider.GetRequiredService<IHostApplicationLifetime>().StopApplication();
            }
            catch (Exception ex)
            {
                installState = InstallState.Failed;
                await Console.Out.WriteLineAsync("SAV解析器palworld-save-tools安装失败：" + ex.Message);
            }
        }
        public static void ConvertSavToJsonFile(string filename, string outputPath)
        {
            if (installState != InstallState.Installed)
            {
                return;
            }
            lock (pythonEngineLock)
            {
                PythonEngine.Initialize();
                Py.GILState gilState = Py.GIL();
                try
                {
                    dynamic converter = Py.Import("palworld_save_tools.commands.convert");
                    converter.convert_sav_to_json(filename, outputPath, true);
                }
                catch (Exception ex)
                {
                    AppUtil.Logger.LogError(ex.Message);
                }
                finally
                {
                    gilState.Dispose();
                    PythonEngine.Shutdown();
                }
            }
        }

        public static void ConvertJsonToSavFile(string filename, string outputPath)
        {
            if (installState != InstallState.Installed)
            {
                return;
            }
            lock (pythonEngineLock)
            {
                PythonEngine.Initialize();
                Py.GILState gilState = Py.GIL();
                try
                {
                    dynamic converter = Py.Import("palworld_save_tools.commands.convert");
                    converter.convert_json_to_sav(filename, outputPath, true);
                }
                catch (Exception ex)
                {
                    AppUtil.Logger.LogError(ex.Message);
                }
                finally
                {
                    gilState.Dispose();
                    PythonEngine.Shutdown();
                }
            }
        }

        public static string? ConvertSavToJson(string filename)
        {
            if (installState != InstallState.Installed)
            {
                return null;
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"temp")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"temp"));
            }
            string outFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"temp\{AppUtil.Env}_{Guid.NewGuid()}.json");
            ConvertSavToJsonFile(filename, outFile);
            string jsonStr = File.ReadAllText(outFile);
            File.Delete(outFile);
            return jsonStr;
        }

        public static void ConvertJsonToSav(string jsonStr, string outputPath)
        {
            if (installState != InstallState.Installed)
            {
                return;
            }
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"temp")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"temp"));
            }
            string jsonFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"temp\{AppUtil.Env}_{Guid.NewGuid()}.json");
            File.WriteAllText(jsonFile, jsonStr);
            ConvertJsonToSavFile(jsonFile, outputPath);
            File.Delete(jsonFile);
        }
    }
}
