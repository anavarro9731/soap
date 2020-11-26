namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System.Collections.Generic;
    using CircuitBoard;

    public class FieldMeta
    {
        public string DataType { get; set; }

        public string FieldLabel { get; set; }

        public string FieldName { get; set; }

        public List<Enumeration> Options { get; set; }

        public object Values { get; set; }
    }
}