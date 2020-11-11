namespace Soap.Utility
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class JsonNetSettings
    {
        
        public static JsonSerializerSettings MessageSchemaSerialiserSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Ignore,  //* ignore null values as they equal ignored properties
            DateFormatHandling = DateFormatHandling.IsoDateFormat, TypeNameHandling = TypeNameHandling.Objects,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc, ContractResolver = defaultContractResolver
        };
        
        public static JsonSerializerSettings ApiMessageSerialiserSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Include, NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = DateFormatHandling.IsoDateFormat, TypeNameHandling = TypeNameHandling.Objects, //TODO remove type name handling?
            DateTimeZoneHandling = DateTimeZoneHandling.Utc, ContractResolver = defaultContractResolver
        };

        public static JsonSerializerSettings ApiMessageDeserialisationSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat, TypeNameHandling = TypeNameHandling.Objects,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc, ContractResolver = defaultContractResolver
        };

        private static DefaultContractResolver defaultContractResolver = new CamelCasePropertyNamesContractResolver();
    }
}