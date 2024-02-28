using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PalServerTools.Utils
{
    public static class StringUtil
    {
        public static string ComputeMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    stringBuilder.Append(hashBytes[i].ToString("x2"));
                
                return stringBuilder.ToString();
            }
        }

        public static string GetArgumentValue(this string commandLine, string argumentName)
        {
            if (string.IsNullOrEmpty(commandLine))
                throw new ArgumentException("commandLine cannot be null or empty.", nameof(commandLine));
            if (string.IsNullOrEmpty(argumentName))
                throw new ArgumentException("argumentName cannot be null or empty.", nameof(argumentName));

            // 正常化参数名（确保它以"-"开始）
            if (!argumentName.StartsWith("-"))
                argumentName = $"-{argumentName}";

            // 分割命令行字符串以获取参数列表
            var parts = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // Dict to hold argument-value pairs
            var argumentsDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 遍历所有的部分来填充字典
            string lastKey = null;
            foreach (var part in parts)
            {
                if (part.StartsWith("-"))
                {
                    // 如果参数看起来像是一个键（即，以"-"开始），将其加入字典
                    var key = part;
                    // Handle cases like "-key=value"
                    var keyValue = key.Split(new[] { '=' }, 2);
                    if (keyValue.Length > 1)
                    {
                        argumentsDict[keyValue[0]] = keyValue[1];
                    }
                    else
                    {
                        lastKey = key;
                        argumentsDict[key] = null; // Initially null, value might be the next part
                    }
                }
                else if (lastKey != null)
                {
                    // If the part doesn't start with "-", it is a value for the last key
                    argumentsDict[lastKey] = part;
                    lastKey = null;
                }
            }

            // 检查我们的参数是否在字典中
            if (argumentsDict.TryGetValue(argumentName, out var value))
            {
                return value;
            }

            // 如果找不到参数，返回null或抛出异常
            return null;
        }
    }
}
