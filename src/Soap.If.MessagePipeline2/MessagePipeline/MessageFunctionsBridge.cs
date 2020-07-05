namespace Soap.MessagePipeline.MessagePipeline
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class MessageFunctionsBridge<T> : IMessageFunctionsServerSide where T : ApiMessage
    {
        private readonly IMessageFunctionsClientSide<T> messageFunctionsTyped;

        public MessageFunctionsBridge(IMessageFunctionsClientSide<T> messageFunctionsTyped)
        {
            this.messageFunctionsTyped = messageFunctionsTyped;
        }

        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings() => this.messageFunctionsTyped.GetErrorCodeMapper();

        public Task Handle(ApiMessage msg) => this.messageFunctionsTyped.Handle((T)msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.messageFunctionsTyped.HandleFinalFailure((MessageFailedAllRetries<T>)msg);

        public void Validate(ApiMessage msg) => this.messageFunctionsTyped.Validate((T)msg);
    }
}