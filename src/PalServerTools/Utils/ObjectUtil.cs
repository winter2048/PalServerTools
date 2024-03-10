using Newtonsoft.Json;
using PalServerTools.Models;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace PalServerTools.Utils
{
    public static class ObjectUtil
    {
        public static void SetObject<T>(this T model1, T model2)
        {
            // 获取PalConfigModel的所有属性
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                // 获取属性的值
                property.SetValue(model1, property.GetValue(model2));
            }
        }

        public static bool CompareModels<T>(T model1, T model2)
        {
            // 判断两个对象是否为空
            if (model1 == null && model2 == null)
            {
                return true;
            }
            else if (model1 == null || model2 == null)
            {
                return false;
            }

            // 获取PalConfigModel的所有属性
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                // 获取属性的值
                var value1 = property.GetValue(model1);
                var value2 = property.GetValue(model2);

                // 比较属性值是否相等
                if (!Equals(value1, value2))
                {
                    return false;
                }
            }

            return true;
        }

        public static T DeepCopy<T>(this T obj)
        {
            // 序列化对象为JSON字符串
            var json = JsonConvert.SerializeObject(obj);

            // 反序列化JSON字符串到新的对象实例
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
