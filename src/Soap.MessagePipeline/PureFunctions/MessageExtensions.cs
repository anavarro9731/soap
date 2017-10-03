namespace Soap.MessagePipeline.PureFunctions
{
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public static class MessageExtensions
    {
        public static bool CanChangeState(this IApiMessage message)
        {
            return message is IApiCommand || message is IApiEvent;
        }
    }
}
