namespace Soap.Interfaces.Messages
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RequiredAttribute : Attribute
    {
    }
}