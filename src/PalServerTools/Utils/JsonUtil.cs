using Newtonsoft.Json.Linq;

namespace PalServerTools.Utils
{
    public static class JsonUtil
    {
        /// <summary>
        /// 辅助函数，确保沿着路径的每个节点都存在，如果不存在则创建
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static JToken EnsurePathExists(JToken root, params string[] path)
        {
            JToken current = root;
            foreach (var nodeName in path)
            {
                if (current[nodeName] == null)
                {
                    // 如果节点不存在，则在当前位置创建新的 JObject
                    current[nodeName] = new JObject();
                }
                current = (JToken)current[nodeName];
            }
            return current;
        }
    }
}
