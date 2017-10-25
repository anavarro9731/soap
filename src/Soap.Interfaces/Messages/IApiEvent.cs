namespace Soap.Interfaces.Messages
{
    using System;

    public interface IApiEvent : IApiMessage
    {
        DateTime OccurredAt { get; set; }
    }
}