using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Fyla.Helpers
{
    public static class JsonExtensions
    {
        public static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None
        };

        public static string Serialize<T>(this T value, JsonSerializerSettings settings = null)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string s && (s.TrimStart().StartsWith("{") || s.TrimStart().StartsWith("[")))
            {
                return s;
            }

            return JsonConvert.SerializeObject(value, settings ?? DefaultJsonSettings);
        }

        public static T Deserialize<T>(this string json, JsonSerializerSettings settings = null) where T : class => (T) Deserialize(json, typeof(T), settings);

        public static object Deserialize(this string json, Type type, JsonSerializerSettings settings = null)
        {
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject(json, type, settings ?? DefaultJsonSettings);
        }
    }
}
