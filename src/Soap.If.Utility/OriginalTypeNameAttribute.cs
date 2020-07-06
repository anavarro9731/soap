namespace Soap.Utility
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class OriginalTypeNameAttribute : Attribute
    {
        public string OriginalName { get; }

        public OriginalTypeNameAttribute(string originalName)
        {
            OriginalName = originalName;
        }
    }
}