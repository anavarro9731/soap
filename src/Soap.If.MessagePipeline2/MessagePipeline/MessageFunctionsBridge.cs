namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility;
    using Soap.Utility.Enums;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public class MessageFunctionsBridge<T> : IMessageFunctionsServerSide where T : ApiMessage
    {
        private readonly IMessageFunctionsClientSide<T> messageFunctionsTyped;

        public MessageFunctionsBridge(IMessageFunctionsClientSide<T> messageFunctionsTyped)
        {
            this.messageFunctionsTyped = messageFunctionsTyped;
        }

        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMappings() => this.messageFunctionsTyped.GetErrorCodeMapper ?? new Dictionary<ErrorCode, ErrorCode>();

        public async Task Handle(ApiMessage msg)
        {
            if (msg.Headers.HasStatefulProcessId())
            {
                var permId = msg.Headers.GetStatefulProcessId();
                foreach (var type in this.messageFunctionsTyped.MessageCanContinueTheseStatefulProcesses)
                {
                    OriginalTypeNameAttribute myAttribute =
                        (OriginalTypeNameAttribute) Attribute.GetCustomAttribute(type, typeof (OriginalTypeNameAttribute));

                    var originalName = myAttribute.OriginalName;

                    var currentName = type.FullName;

                    if (originalName == msg.Headers.GetStatefulProcessId().TypeId
                        || currentName == msg.Headers.GetStatefulProcessId().TypeId)
                    {
                        StatefulProcess process = ((StatefulProcess)Activator.CreateInstance(type));
                        await process.ContinueProcess(msg);
                    }
                    else
                    {
                        Guard.Against(true, $"Could not find a Stateful Process with the Id {permId}", ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                    }
                    
                }
            } else 
                await this.messageFunctionsTyped.Handle((T)msg);
        } 

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.messageFunctionsTyped.HandleFinalFailure((MessageFailedAllRetries<T>)msg);
         
        public void Validate(ApiMessage msg) => this.messageFunctionsTyped.Validate((T)msg);
    }
}