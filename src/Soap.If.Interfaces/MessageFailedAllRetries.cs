namespace Soap.Interfaces.Messages
{
    using System;

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        public Guid IdOfMessageThatFailed { get; set; }

        public Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}