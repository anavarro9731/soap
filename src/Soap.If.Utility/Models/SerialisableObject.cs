namespace Soap.If.Utility.Models
{
    using System;
    using System.Text.Json;
    using Soap.If.Interfaces.Messages;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class SerialisableObject
    {
        public SerialisableObject(object x)
        {
            ObjectData = x.ToJson();
            TypeName = x.GetType().AssemblyQualifiedName;
        }

        public string ObjectData { get; internal set; }

        public string TypeName { get; internal set; }

        public T Deserialise<T>() where T : class
        {
            T obj = JsonSerializer.Deserialize(ObjectData, Type.GetType(TypeName)).As<T>();
            return obj;
        }

    }
}