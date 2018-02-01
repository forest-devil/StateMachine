using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine.Json
{
    /// <summary>
    /// 用于Json.NET的序列化/反序列化转换器
    /// </summary>
    public class StringStatusJsonConverter : JsonConverter
    {
        /// <summary>
        /// 是否可以转换
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <returns>是/否</returns>
        public override bool CanConvert(Type objectType) => typeof(IStatus).IsAssignableFrom(objectType);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="reader">JsonReader实例</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="existingValue">已存在的值</param>
        /// <param name="serializer">JsonSerializer实例</param>
        /// <returns>发序列化后的对象</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = (JToken.Load(reader)).ToString();
            var statusEnumType = objectType.BaseType.GetGenericArguments()[0];
            var enumValue = Enum.Parse(statusEnumType, value);
            if (!objectType.IsAbstract)
            {
                return Activator.CreateInstance(objectType, enumValue);
            }
            else
            {
                return objectType.GetMethod("CreateInstance").Invoke(null, new[] { enumValue });
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="writer">JsonWriter对象</param>
        /// <param name="value">待序列化对象</param>
        /// <param name="serializer">JsonSerializer实例</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => writer.WriteValue(value.ToString());
    }
}