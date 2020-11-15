namespace Soap.Utility.Functions.Extensions
{
    using Soap.Interfaces.Messages;

    public static class MessageFailedAllRetriesExt
    {
        public static ApiMessage ToApiMessage(this MessageFailedAllRetries message)
        {
            return message.Map(
                x => message.SerialisedMessage.FromJson<ApiMessage>(
                    SerialiserIds.FromKey(message.SerialiserId),
                    message.TypeName));
        }
    }
}