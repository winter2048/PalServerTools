using System.Reflection;
using System.Text;

namespace PalServerTools.Utils
{
    public static class ResourceUtil
    {
        public static string GetEmbeddedResourceAsString(string resourceName)
        {
            // 获取调用此方法的程序集
            Assembly assembly = Assembly.GetCallingAssembly();

            // 读取资源内容
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Resource '{resourceName}' not found. Ensure the name is correct and the resource is set to 'EmbeddedResource'.");
                }
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
