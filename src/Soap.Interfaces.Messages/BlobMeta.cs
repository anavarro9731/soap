namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IsImageAttribute : Attribute
    {
    }

    public class BlobMeta
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }
    }
}
