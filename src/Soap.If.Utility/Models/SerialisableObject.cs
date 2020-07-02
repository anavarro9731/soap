namespace Soap.Utility.Models
{
    using System.Text.Json.Serialization;
    using Soap.Utility.Functions.Extensions;

    public class SerialisableObject
    {
        public SerialisableObject(object x)
        {
            ObjectData = x.ToJson();
            TypeName = x.GetType().AssemblyQualifiedName;
        }

        public SerialisableObject() { }

        [JsonInclude]
        public string ObjectData { get; internal set; }

        [JsonInclude]
        public string TypeName { get; internal set; }

        public T Deserialise<T>() where T : class
        {
            return ObjectData.FromJsonToInterface<T>(TypeName);
        }
    }
}