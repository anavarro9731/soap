namespace Soap.If.Interfaces
{
    using System;
    using Soap.If.Interfaces.Messages;

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        public Guid IdOfMessageThatFailed { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}