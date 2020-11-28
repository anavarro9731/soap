namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard;

    public class FieldMeta
    {
        public string? DataType { get; set; }

        public string? FieldLabel { get; set; }

        public string? FieldName { get; set; }

        public object InitialValue { get; set; }

        public bool? Required { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RequiredAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LabelAttribute : Attribute
    {
        public string Label;

        public LabelAttribute(string label)
        {
            this.Label = label;
        }
    }
}
