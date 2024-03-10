
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PalServerTools.Models;
using PalServerTools.Utils;
using System.Configuration;
using System.Reactive.Joins;
using System.Text;
using System.Text.RegularExpressions;

namespace PalServerTools.Data
{
    public class PalConfigService
    {
        private readonly IConfiguration _configuration;
        private ToolsConfigModel _toolsConfigModel;
        private PalConfigModel _tefaultPalConfig;
        private PalConfigModel _palConfig;

        private string appSettingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        private string? worldOptionJsonStr;
        private string? worldOptionPath;

        public PalConfigModel DefaultPalConfig { get { return _tefaultPalConfig; } }
        public PalConfigModel PalConfig { get { return _palConfig; } }
        public ToolsConfigModel ToolsConfig { get { return _toolsConfigModel; } }

        public PalConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
            _toolsConfigModel = GetToolsConfig();
            _tefaultPalConfig = GetDefaultPalConfig();
            _palConfig = GetPalConfig();

            FileUtil.WatchFile(Path.Combine(ToolsConfig.PalServerPath, "DefaultPalWorldSettings.ini"), (obj, ev) => { _tefaultPalConfig = GetDefaultPalConfig(); });
            FileUtil.WatchFile(ToolsConfig.PalServerPath, @"WorldOption.sav", (obj, ev) => { _palConfig = GetPalConfig(); }, true);
            FileUtil.WatchFile(Path.Combine(ToolsConfig.PalServerPath, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini"), (obj, ev) => { _palConfig = GetPalConfig(); });
            FileUtil.WatchFile(appSettingPath, (obj, ev) =>
            {
                _toolsConfigModel = GetToolsConfig();
                FileUtil.WatchFile(Path.Combine(ToolsConfig.PalServerPath, "DefaultPalWorldSettings.ini"), (obj, ev) => { _tefaultPalConfig = GetDefaultPalConfig(); });
                FileUtil.WatchFile(ToolsConfig.PalServerPath, @"WorldOption.sav", (obj, ev) => { _palConfig = GetPalConfig(); }, true);
                FileUtil.WatchFile(Path.Combine(ToolsConfig.PalServerPath, @"Pal\Saved\Config\WindowsServer\PalWorldSettings.ini"), (obj, ev) => { _palConfig = GetPalConfig(); });
            });
        }

        private PalConfigModel GetDefaultPalConfig()
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
            PalConfigModel palConfig = new PalConfigModel();
            // ����Ĭ��ֵ
            foreach (var kvp in defaultConfigData)
            {
                // ���÷����ҵ����ԣ����������ǵ�ֵ
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        var targetType = property.PropertyType;
                        var value = Enum.Parse(targetType, kvp.Value, true);
                        property.SetValue(palConfig, value);
                    }
                    else
                    {
                        // �������Ե����ͽ�����Ӧ��ֵת��
                        var targetType = property.PropertyType;
                        var value = Convert.ChangeType(kvp.Value, targetType);
                        property.SetValue(palConfig, value);
                    }
                }
            }

            return palConfig;
        }

        private PalConfigModel GetPalConfig()
        {
            // ����WorldOption.sav�ļ�����ת��Ϊ�ֵ��ʽ
            var worldOptionDir = Path.Combine(ToolsConfig.PalServerPath, @"Pal\Saved\SaveGames\0");
            if (Directory.Exists(worldOptionDir))
            {
                var savePath = Directory.GetDirectories(worldOptionDir).FirstOrDefault();
                if (savePath != null)
                {
                    worldOptionPath = Path.Combine(savePath, @"WorldOption.sav");
                }
            }

            Dictionary<string, string> worldOptionConfigData = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(worldOptionPath) && File.Exists(worldOptionPath))
            {

                worldOptionJsonStr = PalSavUtil.ConvertSavToJson(worldOptionPath);
                if (!string.IsNullOrWhiteSpace(worldOptionJsonStr))
                {
                    var worldOption = JsonConvert.DeserializeObject<JToken>(worldOptionJsonStr);
                    var worldOptionKv = worldOption?["properties"]?["OptionWorldData"]?["value"]?["Settings"]?["value"];
                    if (worldOptionKv != null)
                    {
                        foreach (JProperty property in worldOptionKv.Children<JProperty>())
                        {
                            string key = property.Name;
                            JToken propertyValue = property.Value["value"];
                            JToken propertyType = property.Value["type"];

                            string? value;
                            if (propertyType?.Value<string>() == "EnumProperty")
                            {
                                value = propertyValue?["value"]?.Value<string>()?.Split("::").Last();
                            }
                            else
                            {
                                value = propertyValue?.Value<string>();
                            }
                            worldOptionConfigData[key] = value ?? "";
                        }
                    }
                }
            }

            // ����PalWorldSettings.ini�ļ�����ת��Ϊ�ֵ��ʽ
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

            // ���ֵ�ת��ΪPalConfigModel����
            PalConfigModel palConfig = ObjectUtil.DeepCopy(DefaultPalConfig);

            // ����PalWorldSettings.iniʵ��ֵ
            foreach (var kvp in configData)
            {
                // ���÷����ҵ����ԣ����������ǵ�ֵ
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        var targetType = property.PropertyType;
                        var value = Enum.Parse(targetType, kvp.Value, true);
                        property.SetValue(palConfig, value);
                    }
                    else
                    {
                        // �������Ե����ͽ�����Ӧ��ֵת��
                        var targetType = property.PropertyType;
                        var value = Convert.ChangeType(kvp.Value, targetType);
                        property.SetValue(palConfig, value);
                    }
                }
            }

            // ����WorldOption.savʵ��ֵ
            foreach (var kvp in worldOptionConfigData)
            {
                // ���÷����ҵ����ԣ����������ǵ�ֵ
                var property = typeof(PalConfigModel).GetProperty(kvp.Key);
                if (property != null)
                {
                    if (property.PropertyType.BaseType == typeof(Enum))
                    {
                        var targetType = property.PropertyType;
                        var value = Enum.Parse(targetType, kvp.Value, true);
                        property.SetValue(palConfig, value);
                    }
                    else
                    {
                        // �������Ե����ͽ�����Ӧ��ֵת��
                        var targetType = property.PropertyType;
                        var value = Convert.ChangeType(kvp.Value, targetType);
                        property.SetValue(palConfig, value);
                    }
                }
            }

            return palConfig;
        }

        private ToolsConfigModel GetToolsConfig()
        {
            ((IConfigurationRoot)_configuration).Reload();
            ToolsConfigModel toolsConfig = new ToolsConfigModel();
            var propertys = typeof(ToolsConfigModel).GetProperties();

            foreach (var property in propertys)
            {

                var value = _configuration.GetSection("ToolsConfig").GetValue<object>(property.Name);
                if (value != null)
                {
                    var targetType = property.PropertyType;
                    property.SetValue(toolsConfig, Convert.ChangeType(value, targetType));
                }

            }
            return toolsConfig;
        }

        public void ReLoad()
        {
            _toolsConfigModel = GetToolsConfig();
            _tefaultPalConfig = GetDefaultPalConfig();
            _palConfig = GetPalConfig();
        }

        public async Task ToolsConfigSave(ToolsConfigModel toolsConfig)
        {
            var appSettingsJson = File.ReadAllText("appsettings.json");
            JObject configJson = JObject.Parse(appSettingsJson);
            configJson["ToolsConfig"] = JObject.FromObject(toolsConfig);
            var modifiedAppSettingsJson = JsonConvert.SerializeObject(configJson, Formatting.Indented);
            await File.WriteAllTextAsync("appsettings.json", modifiedAppSettingsJson);
        }

        public async Task PalConfigSave(PalConfigModel palConfig)
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
                var value = property.GetValue(palConfig);
                if (value != null)
                {
                    string valueStr;
                    if (value is string)
                    {
                        valueStr = @"""" + value.ToString() + @"""";
                    }
                    else
                    {
                        valueStr = Convert.ChangeType(value, property.PropertyType).ToString() ?? "";
                    }

                    optionSettings.Add($"{property.Name}={valueStr}");
                }
            }
            configBuilder.AppendLine($"OptionSettings=({string.Join(",", optionSettings)})");

            // ���浽PalWorldSettings.ini�ļ�
            await File.WriteAllTextAsync(palServerConfigPath, configBuilder.ToString());

            // ���浽WorldOption.sav�ļ�
            if (!string.IsNullOrWhiteSpace(worldOptionPath))
            {
                if (string.IsNullOrWhiteSpace(worldOptionJsonStr))
                {
                    worldOptionJsonStr = SyValue.emptyWorldOption;
                }
                var worldOption = JsonConvert.DeserializeObject<JToken>(worldOptionJsonStr);
                var worldOptionKv = worldOption?["properties"]?["OptionWorldData"]?["value"]?["Settings"]?["value"];
                if (worldOptionKv == null)
                {
                    worldOptionKv = JsonUtil.EnsurePathExists(worldOption, "properties", "OptionWorldData", "value", "Settings", "value");
                }
                foreach (var property in typeof(PalConfigModel).GetProperties())
                {
                    var value = property.GetValue(palConfig);
                    if (value != null)
                    {
                        var propertyName = property.Name;

                        // ���������������ɶ�Ӧ��JSON�ṹ
                        if (property.PropertyType.IsEnum)
                        {
                            worldOptionKv[propertyName] = new JObject { ["id"] = null, ["value"] = new JObject { ["type"] = property.PropertyType.Name, ["value"] = $"{property.PropertyType.Name}::{value}" }, ["type"] = "EnumProperty" };
                        }
                        else
                        {
                            switch (Type.GetTypeCode(property.PropertyType))
                            {
                                case TypeCode.Int32:
                                    worldOptionKv[propertyName] = new JObject { ["id"] = null, ["value"] = (int)value, ["type"] = "IntProperty" };
                                    break;
                                case TypeCode.Single:
                                    worldOptionKv[propertyName] = new JObject { ["id"] = null, ["value"] = (float)value, ["type"] = "FloatProperty" };
                                    break;
                                case TypeCode.String:
                                    worldOptionKv[propertyName] = new JObject { ["id"] = null, ["value"] = (string)value, ["type"] = "StrProperty" };
                                    break;
                                case TypeCode.Boolean:
                                    worldOptionKv[propertyName] = new JObject { ["id"] = null, ["value"] = (bool)value, ["type"] = "BoolProperty" };
                                    break;
                                default:
                                    Console.WriteLine($"Warning: Property '{propertyName}' with type '{property.PropertyType}' is not supported for JSON serialization.");
                                    break;
                            }
                        }
                    }
                }
                var str = JsonConvert.SerializeObject(worldOption);
                PalSavUtil.ConvertJsonToSav(str, worldOptionPath);
            }
        }

        public void RestoreDefaultPalConfig(PalConfigModel palConfig)
        {
            palConfig.SetObject(ObjectUtil.DeepCopy(DefaultPalConfig));
        }

        public async Task Save(ToolsConfigModel toolsConfig, PalConfigModel palConfig)
        {
            if (!ObjectUtil.CompareModels(ToolsConfig, toolsConfig))
            {
                await ToolsConfigSave(toolsConfig);
            }
            if (!ObjectUtil.CompareModels(PalConfig, palConfig))
            {
                await PalConfigSave(palConfig);
            }
        }
    }
}