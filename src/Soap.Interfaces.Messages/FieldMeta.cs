namespace Soap.Api.Sample.Messages.Commands.UI
{
    public class FieldMeta
    {
        public string Caption { get; set; }

        public string DataType { get; set; }

        public object InitialValue { get; set; }

        public string Label { get; set; }

        public string Name { get; set; }

        public bool? Required { get; set; }
        
    }
    
    public class ObjectMeta
    {
        public string Path { get; set; }
        
        public string DotNetTypeName { get; set; }
    }
}
