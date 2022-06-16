namespace Soap.Utility.Models
{
    using Newtonsoft.Json;
    using Soap.Utility.Functions.Extensions;

    public class SerialisableObject
    {
        public
            SerialisableObject(
                object x,
                SerialiserIds serialiserId =
                    null) //* ok to use default serialiserId because this serialisation and deserialisation is self-contained in this class
        {
            if (serialiserId == null) serialiserId = SerialiserIds.JsonDotNetDefault;
            var json = x.ToJson(serialiserId);

            ObjectData = json;
            SerialiserId = serialiserId.Key;
            TypeName = x.GetType().ToShortAssemblyTypeName();
        }

        public SerialisableObject()
        {
        }

        [JsonProperty]
        public string ObjectData { get; internal set; }

        [JsonProperty]
        public string SerialiserId { get; internal set; }

        [JsonProperty]
        public string TypeName { get; internal set; }

        public T Deserialise<T>() where T : class
        {
            var obj = ObjectData.FromJson<T>(SerialiserIds.GetInstanceFromKey(SerialiserId), TypeName);
            return obj;
        }
    }

    public static class SerialisableObjectExt
    {
        public static T FromSerialisableObject<T>(this SerialisableObject s) where T : class => s.Deserialise<T>();

        public static SerialisableObject ToSerialisableObject(this object o, SerialiserIds serialiserId = null) => new SerialisableObject(o, serialiserId);
    }
}
