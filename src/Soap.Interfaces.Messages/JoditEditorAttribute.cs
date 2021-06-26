namespace Soap.Interfaces.Messages
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JoditEditorAttribute : Attribute
    {
        public int PixelHeight { get; set; }
    }
}