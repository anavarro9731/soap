namespace Soap.Api.Sample.Messages.Commands.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IsImageAttribute : Attribute
    {
    }

    public class Base64Blob
    {
        
        public string Blob { get; set; }

        public string Name { get; set; }

        public string FullSize { get; set; }

        public long? FullSizeHeight { get; set; }

        public long? FullSizeWidth { get; set; }

        public string Type { get; set; }

        public string Size { get; set; }

        public string Thumb { get; set; }
    }
}
