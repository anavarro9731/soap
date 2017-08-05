namespace Palmtree.ApiPlatform.Interfaces
{
    using System;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public interface IMessageFailedAllRetries : IApiCommand
    {
        Guid IdOfMessageThatFailed { get; set; }
    }
}
