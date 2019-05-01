namespace Soap.If.Interfaces
{
    using System;
    using Soap.If.Interfaces.Messages;

    public class MessageFailedAllRetries<TFailedMessage> : ApiCommand, IMessageFailedAllRetries
    {
        public MessageFailedAllRetries(Guid idOfMessageThatFailed)
        {
            IdOfMessageThatFailed = idOfMessageThatFailed;            
        }

        public MessageFailedAllRetries() { }

        public Guid IdOfMessageThatFailed { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
    
    //TODO: add validation, check that if incoming message has stateful process id that this one does as well
}