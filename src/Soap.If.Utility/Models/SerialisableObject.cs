namespace Soap.Utility.Models
{
    using Soap.Utility.Functions.Extensions;

    public class SerialisableObject
    {
        public SerialisableObject(object x)
        {
            ObjectData = x.ToJson();
            TypeName = x.GetType().AssemblyQualifiedName;
        }

        public SerialisableObject() { }

        public string ObjectData { get; internal set; }

        public string TypeName { get; internal set; }

        public T Deserialise<T>() where T : class
        {
            return ObjectData.FromJsonToInterface<T>(TypeName);
        }
    }
}