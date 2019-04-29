namespace Soap.If.Interfaces.Messages
{
    using System;

    public interface IApiEvent : IApiMessage
    {
        DateTime OccurredAt { get; set; }
    }
}