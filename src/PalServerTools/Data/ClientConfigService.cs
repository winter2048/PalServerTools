using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PalServerTools.Models;
using PalServerTools.Utils;
using Microsoft.AspNetCore.Components;

namespace PalServerTools.Data
{
    public class ClientConfigService
    {
        private ICookieUtil _cookieUtil { get; set; }

        public ClientConfigModel ClientConfig;

        internal event Action OnSave;

        public ClientConfigService(ICookieUtil cookieUtil)
        {
            _cookieUtil = cookieUtil;
        }

        public async Task Init()
        {
            if (ClientConfig == null)
            {
                ClientConfig = await GetClientConfig();
            }
        }

        private async Task<ClientConfigModel> GetClientConfig()
        {
            ClientConfigModel clientConfig = new ClientConfigModel();
            var propertys = typeof(ClientConfigModel).GetProperties();
            foreach (var property in propertys)
            {
                try
                {
                    var value = await _cookieUtil.GetValueAsync(property.Name);
                    if (value != null)
                    {
                        var targetType = property.PropertyType;
                        property.SetValue(clientConfig, Convert.ChangeType(value, targetType));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return clientConfig;
        }

        public async Task ClientConfigSave()
        {
            foreach (var property in typeof(ClientConfigModel).GetProperties())
            {
                var value = property.GetValue(ClientConfig);
                if (value != null)
                {
                    string valueStr = Convert.ChangeType(value, property.PropertyType).ToString();
                    await _cookieUtil.SetValueAsync(property.Name, valueStr);
                }
            }
        }

        public async Task Save()
        {
            if (!ObjectUtil.CompareModels(ClientConfig, await GetClientConfig()))
            {
                await ClientConfigSave();
                if (OnSave != null)
                {
                    OnSave.Invoke();
                }
            }
        }
    }
}
