namespace Soap.Interfaces.Messages
{
    using System;
    using CircuitBoard.Messages;

    public interface IApiMessage : IMessage
    {
        string IdentityToken { get; set; }

        Guid MessageId { get; set; }

        DateTime? TimeOfCreationAtOrigin { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public static class IApiMessageMaps
    {
        public static bool CanChangeState(this IApiMessage message)
        {
            return message is IApiCommand || message is IApiEvent;
        }
    }
}