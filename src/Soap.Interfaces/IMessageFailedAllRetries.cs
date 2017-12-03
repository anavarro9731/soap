namespace Soap.If.Interfaces
{
    using System;
    using Soap.If.Interfaces.Messages;

    public interface IMessageFailedAllRetries : IApiCommand
    {
        Guid IdOfMessageThatFailed { get; set; }

        Guid? StatefulProcessIdOfMessageThatFailed { get; set; }
    }
}