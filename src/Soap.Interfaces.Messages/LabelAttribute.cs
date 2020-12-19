namespace Soap.Interfaces.Messages
{
    using System;

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