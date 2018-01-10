using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace StateMachine
{
    public class StringStatusJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(Status<,,>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = (JToken.Load(reader)).ToString();
            return Activator.CreateInstance(objectType, s);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}