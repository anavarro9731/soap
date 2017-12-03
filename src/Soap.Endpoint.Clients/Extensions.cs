namespace Soap.Pf.EndpointClients
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class Extensions
    {
        private static readonly JsonSerializerSettings JsonDeserialisationSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects
        };

        public static object FromJson(this string json)
        {
            var obj = JsonConvert.DeserializeObject(json, JsonDeserialisationSettings);
            return obj;
        }

        public static T FromJson<T>(this string json)
        {
            var obj = JsonConvert.DeserializeObject<T>(json, JsonDeserialisationSettings);
            return obj;
        }

        public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
            for (; toCheck != (Type)null && toCheck != typeof(object); toCheck = toCheck.BaseType)
            {
                var type = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == type)
                {
                    return true;
                }
            }
            return false;
        }

        public static string ToJson(this object instance, Formatting formatting = Formatting.None)
        {
            var json = JsonConvert.SerializeObject(instance, formatting, JsonSerializerSettings);
            return json;
        }
    }
}