namespace Soap.If.Interfaces
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class MessageFailedAllRetries<T> : ApiCommand, IMessageFailedAllRetries where T : IApiMessage
    {
        public MessageFailedAllRetries(Guid idOfMessageThatFailed)
        {
            IdOfMessageThatFailed = idOfMessageThatFailed;            
        }

        public Guid IdOfMessageThatFailed { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }

    //TODO: add validation, check that if incoming message has stateful process id that this one does as well
}