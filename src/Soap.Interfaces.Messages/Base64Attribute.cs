namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Base64Attribute : Attribute
    {
        public enum BlobType
        {
            File,
            Image
        }

        public BlobType Type { get; set; }

        public Base64Attribute(BlobType type)
        {
            Type = type;
        }
    }
}
