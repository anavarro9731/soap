namespace Soap.Context.MessageMapping
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Models.PureFunctions.Extensions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Blended;

    public class MessageFunctionsBridge<T> : IMessageFunctionsServerSide where T : ApiMessage
    {
        private readonly IMessageFunctionsClientSide<T> messageFunctionsTyped;

        public MessageFunctionsBridge(IMessageFunctionsClientSide<T> messageFunctionsTyped)
        {
            this.messageFunctionsTyped = messageFunctionsTyped;
        }

        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings() =>
            this.messageFunctionsTyped.GetErrorCodeMapper ?? new Dictionary<ErrorCode, ErrorCode>();

        public async Task Handle(ApiMessage msg)
        {
            if (msg.Headers.HasStatefulProcessId())
            {
                //* is continuation
                foreach (var process in this.messageFunctionsTyped.HandleWithTheseStatefulProcesses)
                {
                    var asIContinueStatefulProcess = (IContinueStatefulProcess)process;
                    ;
                    if (IsOfCorrectType(asIContinueStatefulProcess))
                    {
                        await asIContinueStatefulProcess.ContinueProcess((T)msg);
                    }
                }
            }
            else
            {
                await this.messageFunctionsTyped.Handle((T)msg);
            }

            bool IsOfCorrectType(IContinueStatefulProcess process) =>
                process.GetType().InheritsOrImplements(Type.GetType(msg.Headers.GetStatefulProcessId().Value.TypeId));
        }

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.messageFunctionsTyped.HandleFinalFailure((MessageFailedAllRetries<T>)msg);

        public void Validate(ApiMessage msg) => this.messageFunctionsTyped.Validate((T)msg);
    }
}