namespace Soap.Utility.Models
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;
    using Soap.Utility.Functions.Extensions;

    public class SerialisableObject
    {
        public SerialisableObject(object x)
        {
            ObjectData = x.ToNewtonsoftJson();
            TypeName = x.GetType().AssemblyQualifiedName;
        }

        public SerialisableObject() { }

        [JsonProperty]
        public string ObjectData { get; internal set; }

        [JsonProperty]
        public string TypeName { get; internal set; }

        public T Deserialise<T>() where T : class
        {
            return ObjectData.FromJsonToInterface<T>(TypeName);
        }
    }
}