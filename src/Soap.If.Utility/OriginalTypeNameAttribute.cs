namespace Soap.Utility
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class OriginalTypeNameAttribute : Attribute
    {
        public OriginalTypeNameAttribute(string originalName)
        {
            OriginalName = originalName;
        }

        public string OriginalName { get; }
    }
}