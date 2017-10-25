namespace Soap.Interfaces
{
    using System;
    using Soap.Interfaces.Messages;

    public class MessageFailedAllRetries<T> : ApiCommand, IMessageFailedAllRetries where T : IApiMessage
    {
        public MessageFailedAllRetries(Guid idOfMessageThatFailed)
        {
            IdOfMessageThatFailed = idOfMessageThatFailed;
        }

        public Guid IdOfMessageThatFailed { get; set; }
    }
}