namespace Soap.MessagePipeline.MessagePipeline
{
    using System;
    using System.Collections.Generic;
    using DataStore.Models.PureFunctions;
    using Soap.Interfaces.Messages;

    public class MapMessagesToFunctions
    {
        private readonly Dictionary<Type, IMessageFunctions> messageMappings = new Dictionary<Type, IMessageFunctions>();

        internal void AddMapping<TMessage>(IMessageFunctions messageFunctions)
        {
            var messageType = typeof(TMessage);

            Guard.Against(this.messageMappings.ContainsKey(messageType), "Cannot map the same message type twice");

            this.messageMappings.Add(messageType, messageFunctions);
        }

        internal IMessageFunctions MapMessage(ApiMessage message)
        {
            var messageType = message.GetType();

            Guard.Against(!this.messageMappings.ContainsKey(messageType), "Could not find a mapping for message this message");

            return this.messageMappings[messageType];
        }
    }
}