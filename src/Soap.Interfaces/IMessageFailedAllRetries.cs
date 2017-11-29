namespace Soap.Interfaces
{
    using System;
    using Soap.Interfaces.Messages;

    public interface IMessageFailedAllRetries : IApiCommand
    {
        Guid IdOfMessageThatFailed { get; set; }

        Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}