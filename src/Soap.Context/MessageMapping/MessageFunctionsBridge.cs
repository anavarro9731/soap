namespace Soap.Context.MessageMapping
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Models.PureFunctions.Extensions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;

    public class MessageFunctionsBridge<T> : IMessageFunctionsServerSide where T : ApiMessage
    {
        private readonly IMessageFunctionsClientSide<T> messageFunctionsTyped;

        public MessageFunctionsBridge(IMessageFunctionsClientSide<T> messageFunctionsTyped)
        {
            this.messageFunctionsTyped = messageFunctionsTyped;
        }
        public async Task Handle(ApiMessage msg)
        {
            if (msg.Headers.HasStatefulProcessId())
            {
                //* is continuation
                foreach (var process in this.messageFunctionsTyped.HandleWithTheseStatefulProcesses)
                {
                    var asIContinueStatefulProcess = (IContinueStatefulProcess)process;
                    
                    Guard.Against(asIContinueStatefulProcess == null, $"This message contains a Stateful Process Id but no matching stateful process could be found in the {nameof(IMessageFunctionsClientSide<ApiMessage>.HandleWithTheseStatefulProcesses)} collection", "This message continues a workflow but data from the previous steps could not be retrieved");
                    
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
            this.messageFunctionsTyped.HandleFinalFailure(msg);

        public void Validate(ApiMessage message) => this.messageFunctionsTyped.Validate((T)message);
    }
}
