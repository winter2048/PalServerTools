
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PalServerTools.Models;

namespace PalServerTools.Data
{
    public class ConfigService
    {
        private readonly IConfiguration _configuration;

        public PalConfigModel PalConfig;
        public ToolsConfigModel ToolsConfig;

        public ConfigService(IConfiguration configuration) {
            _configuration = configuration;
            ToolsConfig = GetToolsConfig();
            PalConfig = GetPalConfig();
        }

        private PalConfigModel GetPalConfig()
        {
            var palServerConfigPath = Path.Combine(ToolsConfig.PalServerPath, "Pal\\Saved\\Config\\WindowsServer\\PalWorldSettings.ini");
            string fileContent = File.ReadAllText(palServerConfigPath);

            // �����ļ�����ת��Ϊ�ֵ��ʽ
            Dictionary<string, string> configData = new Dictionary<string, string>();
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.StartsWith("OptionSettings="))
                {
                    string optionSettings = line.Substring(15);
                    string[] keyValuePairs = optionSettings.Split(',');
                    foreach (string keyValuePair in keyValuePairs)
                    {
                        string[] keyValue = keyValuePair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            string key = keyValue[0].Trim();
                            string value = keyValue[1].Trim();
                            configData[key] = value.Replace(@"""","");
                        }
                    }
                    break;
                }
            }

            // ���ֵ�ת��ΪPalConfigModel����
            PalConfigModel palConfig = new PalConfigModel();
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


        public void ToolsConfigSave()
        {
            var appSettingsJson = File.ReadAllText("appsettings.json");
            JObject configJson = JObject.Parse(appSettingsJson);
            configJson["ToolsConfig"] = JObject.FromObject(ToolsConfig);
            var modifiedAppSettingsJson = JsonConvert.SerializeObject(configJson, Formatting.Indented);
            File.WriteAllText("appsettings.json", modifiedAppSettingsJson);
        }
    }
}