
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PalServerTools.Models;
using PalServerTools.Utils;
using System.Reactive.Joins;
using System.Text;
using System.Text.RegularExpressions;

namespace PalServerTools.Data
{
    public class PalConfigService
    {
        private readonly IConfiguration _configuration;

        public PalConfigModel PalConfig;
        public ToolsConfigModel ToolsConfig;

        public PalConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
            ToolsConfig = GetToolsConfig();
            PalConfig = GetPalConfig();
        }

        private PalConfigModel GetPalConfig()
        {
            // 解析文件内容转换为字典格式
            string fileContent = "";
            var palServerConfigPath = Path.Combine(ToolsConfig.PalServerPath, "Pal\\Saved\\Config\\WindowsServer\\PalWorldSettings.ini");
            if (File.Exists(palServerConfigPath))
            {
                fileContent = File.ReadAllText(palServerConfigPath);
            }
            Dictionary<string, string> configData = new Dictionary<string, string>();
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.StartsWith("OptionSettings="))
                {
                    Match match = Regex.Match(line, @"(?<=OptionSettings=\()(.*?)(?=\))");
                    if (match.Success)
                    {
                        string optionSettings = match.Groups[0].Value;
                        string[] keyValuePairs = optionSettings.Split(',');
                        foreach (string keyValuePair in keyValuePairs)
                        {
                            string[] keyValue = keyValuePair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0].Trim();
                                string value = keyValue[1].Trim();
                                configData[key] = value.Replace(@"""", "");
                            }
                        }
                    }
                    break;
                }
            }

            // 解析文件内容转换为字典格式 - 默认配置 DefaultPalWorldSettings.ini
            string defaultFileContent = "";
            var defaultPalServerConfigPath = Path.Combine(ToolsConfig.PalServerPath, "DefaultPalWorldSettings.ini");
            if (File.Exists(defaultPalServerConfigPath))
            {
                defaultFileContent = File.ReadAllText(defaultPalServerConfigPath);
            }
            Dictionary<string, string> defaultConfigData = new Dictionary<string, string>();
            string[] defaultLines = defaultFileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in defaultLines)
            {
                if (line.StartsWith("OptionSettings="))
                {
                    Match match = Regex.Match(line, @"(?<=OptionSettings=\()(.*?)(?=\))");
                    if (match.Success)
                    {
                        string optionSettings = match.Groups[0].Value;
                        string[] keyValuePairs = optionSettings.Split(',');
                        foreach (string keyValuePair in keyValuePairs)
                        {
                            string[] keyValue = keyValuePair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0].Trim();
                                string value = keyValue[1].Trim();
                                defaultConfigData[key] = value.Replace(@"""", "");
                            }
                        }
                    }
                    break;
                }
            }

            // 将字典转换为PalConfigModel对象
            PalConfigModel palConfig = new PalConfigModel();
            // 设置默认值
            foreach (var kvp in defaultConfigData)
            {
                // 利用反射找到属性，并设置它们的值
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    // 根据属性的类型进行相应的值转换
                    var targetType = property.PropertyType;
                    var value = Convert.ChangeType(kvp.Value, targetType);
                    property.SetValue(palConfig, value);
                }
            }
            // 设置实际值
            foreach (var kvp in configData)
            {
                // 利用反射找到属性，并设置它们的值
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    // 根据属性的类型进行相应的值转换
                    var targetType = property.PropertyType;
                    var value = Convert.ChangeType(kvp.Value, targetType);
                    property.SetValue(palConfig, value);
                }
            }

            return palConfig;
        }

        private ToolsConfigModel GetToolsConfig()
        {
            ToolsConfigModel toolsConfig = new ToolsConfigModel();
            var propertys = typeof(ToolsConfigModel).GetProperties();
            foreach (var property in propertys)
            {
                var value = _configuration.GetSection("ToolsConfig").GetValue<object>(property.Name);
                if (value != null) {
                    var targetType = property.PropertyType;
                    property.SetValue(toolsConfig, Convert.ChangeType(value, targetType));
                }
               
            }
            return toolsConfig;
        }

        public void ReloadPalConfig()
        {
            PalConfig = GetPalConfig();
        }

        public async Task ToolsConfigSave()
        {
            var appSettingsJson = File.ReadAllText("appsettings.json");
            JObject configJson = JObject.Parse(appSettingsJson);
            configJson["ToolsConfig"] = JObject.FromObject(ToolsConfig);
            var modifiedAppSettingsJson = JsonConvert.SerializeObject(configJson, Formatting.Indented);
            await File.WriteAllTextAsync("appsettings.json", modifiedAppSettingsJson);
        }

        public async Task PalConfigSave()
        {
            var palServerConfigDir = Path.Combine(ToolsConfig.PalServerPath, "Pal\\Saved\\Config\\WindowsServer");
            if (!Directory.Exists(palServerConfigDir))
            {
                Directory.CreateDirectory(palServerConfigDir);
            }
            var palServerConfigPath = Path.Combine(palServerConfigDir, "PalWorldSettings.ini");

            // 将PalConfigModel转换为Ini文件格式的字符串
            StringBuilder configBuilder = new StringBuilder();
            configBuilder.AppendLine("[/Script/Pal.PalGameWorldSettings]");

            List<string> optionSettings = new List<string>();
            foreach (var property in typeof(PalConfigModel).GetProperties())
            {
                var value = property.GetValue(PalConfig);
                if (value != null)
                {
                    string valueStr;
                    if (value is string)
                    {
                        valueStr = @"""" + value.ToString() + @"""";
                    }
                    else
                    {
                        valueStr = Convert.ChangeType(value, property.PropertyType).ToString();
                    }

                    optionSettings.Add($"{property.Name}={valueStr}");
                }
            }
            configBuilder.AppendLine($"OptionSettings=({string.Join(",", optionSettings)})");
          
            // 保存到PalWorldSettings.ini文件
            await File.WriteAllTextAsync(palServerConfigPath, configBuilder.ToString());
        }

        public void RestoreDefaultPalConfig()
        {
            // 解析文件内容转换为字典格式 - 默认配置 DefaultPalWorldSettings.ini
            string defaultFileContent = "";
            var defaultPalServerConfigPath = Path.Combine(ToolsConfig.PalServerPath, "DefaultPalWorldSettings.ini");
            if (File.Exists(defaultPalServerConfigPath))
            {
                defaultFileContent = File.ReadAllText(defaultPalServerConfigPath);
            }
            Dictionary<string, string> defaultConfigData = new Dictionary<string, string>();
            string[] defaultLines = defaultFileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in defaultLines)
            {
                if (line.StartsWith("OptionSettings="))
                {
                    Match match = Regex.Match(line, @"(?<=OptionSettings=\()(.*?)(?=\))");
                    if (match.Success)
                    {
                        string optionSettings = match.Groups[0].Value;
                        string[] keyValuePairs = optionSettings.Split(',');
                        foreach (string keyValuePair in keyValuePairs)
                        {
                            string[] keyValue = keyValuePair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                string key = keyValue[0].Trim();
                                string value = keyValue[1].Trim();
                                defaultConfigData[key] = value.Replace(@"""", "");
                            }
                        }
                    }
                    break;
                }
            }

            // 将字典转换为PalConfigModel对象
            PalConfigModel defaultPalConfig = new PalConfigModel();
            // 设置默认值
            foreach (var kvp in defaultConfigData)
            {
                // 利用反射找到属性，并设置它们的值
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    // 根据属性的类型进行相应的值转换
                    var targetType = property.PropertyType;
                    var value = Convert.ChangeType(kvp.Value, targetType);
                    property.SetValue(defaultPalConfig, value);
                }
            }
            PalConfig = defaultPalConfig;
        }

        public async Task Save()
        {
            if (!ObjectUtil.CompareModels(ToolsConfig, GetToolsConfig()))
            {
                 await ToolsConfigSave();
            }
            if (!ObjectUtil.CompareModels(PalConfig, GetPalConfig()))
            {
                await PalConfigSave();
            }
        }
    }
}