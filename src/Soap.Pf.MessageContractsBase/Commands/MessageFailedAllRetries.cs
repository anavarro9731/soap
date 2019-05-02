namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using Soap.If.Interfaces;
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

        public override void Validate()
        {
            
        }
    }

}