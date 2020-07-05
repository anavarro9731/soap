namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using DataStore.Models.PureFunctions;
    using Soap.Interfaces;

    public class MapMessagesToFunctions
    {
        private readonly Dictionary<Type, IMessageFunctionsServerSide> messageMappings = new Dictionary<Type, IMessageFunctionsServerSide>();

        public void AddMapping<TMessage>(IMessageFunctionsClientSide<TMessage> messageFunctions) where TMessage: ApiMessage
        {
            var messageType = typeof(TMessage);

            Guard.Against(this.messageMappings.ContainsKey(messageType), "Cannot map the same message type twice");

            this.messageMappings.Add(messageType, new MessageFunctionsBridge<TMessage>(messageFunctions));
        }

        internal IMessageFunctionsServerSide MapMessage(ApiMessage message)
        {
            var messageType = message.GetType();

            Guard.Against(!this.messageMappings.ContainsKey(messageType), "Could not find a mapping for message this message");

            return this.messageMappings[messageType];
        }
    }
}