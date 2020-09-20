namespace Soap.Interfaces
{
    using CircuitBoard.Messages;
    using Newtonsoft.Json;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Models;

    public class MessageFailedAllRetries<TFailedMessage> : MessageFailedAllRetries where TFailedMessage : ApiMessage
    {
        public MessageFailedAllRetries(TFailedMessage message)
        {
            SerialisableMessage = new SerialisableObject(message);
        }

        public MessageFailedAllRetries()
        {
        }
        
        public override ApiPermission Permission { get; }
    }

    public abstract class MessageFailedAllRetries : ApiCommand
    {
        [JsonProperty]
        protected SerialisableObject SerialisableMessage { get; set; }

        public ApiMessage FailedMessage => SerialisableMessage.Deserialise<ApiMessage>();
        
    }
}