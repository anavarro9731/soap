namespace Soap.Interfaces.Messages
{
    public class FieldMeta
    {
        public string Caption { get; set; }

        public string DataType { get; set; }

        public object InitialValue { get; set; }
        
        public object Options { get; set; }

        public string Label { get; set; }

        public string Name { get; set; }
        
        public string PropertyName { get; set; }
        
        public bool? Required { get; set; }
        
    }
    
    public class ObjectMeta
    {
        public string Path { get; set; }
        
        public string DotNetTypeName { get; set; }
    }
}
