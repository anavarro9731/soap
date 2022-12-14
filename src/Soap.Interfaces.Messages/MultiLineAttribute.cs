namespace Soap.Interfaces.Messages
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MultiLineAttribute : Attribute
    {
        public int PixelHeight { get; set; }
    }
}
