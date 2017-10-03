namespace Soap.Interfaces
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class MessageFailedAllRetries<T> : ApiCommand, IMessageFailedAllRetries where T : IApiMessage
    {
        public MessageFailedAllRetries(Guid idOfMessageThatFailed)
        {
            IdOfMessageThatFailed = idOfMessageThatFailed;
        }

        public Guid IdOfMessageThatFailed { get; set; }
    }
}
