namespace Soap.Interfaces
{
    using System;
    using Soap.Interfaces.Messages;

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        public Guid IdOfMessageThatFailed { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}