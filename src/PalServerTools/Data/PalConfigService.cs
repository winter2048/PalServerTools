
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
            // �����ļ�����ת��Ϊ�ֵ��ʽ
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

            // �����ļ�����ת��Ϊ�ֵ��ʽ - Ĭ������ DefaultPalWorldSettings.ini
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

            // ���ֵ�ת��ΪPalConfigModel����
            PalConfigModel palConfig = new PalConfigModel();
            // ����Ĭ��ֵ
            foreach (var kvp in defaultConfigData)
            {
                // ���÷����ҵ����ԣ����������ǵ�ֵ
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    // �������Ե����ͽ�����Ӧ��ֵת��
                    var targetType = property.PropertyType;
                    var value = Convert.ChangeType(kvp.Value, targetType);
                    property.SetValue(palConfig, value);
                }
            }
            // ����ʵ��ֵ
            foreach (var kvp in configData)
            {
                // ���÷����ҵ����ԣ����������ǵ�ֵ
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    // �������Ե����ͽ�����Ӧ��ֵת��
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

            // ��PalConfigModelת��ΪIni�ļ���ʽ���ַ���
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
          
            // ���浽PalWorldSettings.ini�ļ�
            await File.WriteAllTextAsync(palServerConfigPath, configBuilder.ToString());
        }

        public void RestoreDefaultPalConfig()
        {
            // �����ļ�����ת��Ϊ�ֵ��ʽ - Ĭ������ DefaultPalWorldSettings.ini
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

            // ���ֵ�ת��ΪPalConfigModel����
            PalConfigModel defaultPalConfig = new PalConfigModel();
            // ����Ĭ��ֵ
            foreach (var kvp in defaultConfigData)
            {
                // ���÷����ҵ����ԣ����������ǵ�ֵ
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    // �������Ե����ͽ�����Ӧ��ֵת��
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