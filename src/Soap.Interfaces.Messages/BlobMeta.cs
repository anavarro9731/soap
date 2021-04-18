namespace Soap.Interfaces.Messages
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

        public string BlobMetaMarker { get; set; } = "20fb62ff-9dd3-436e-a356-eceb335c2572";
    }
}
