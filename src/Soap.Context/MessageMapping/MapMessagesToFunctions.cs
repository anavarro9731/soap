namespace Soap.Context.MessageMapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    
    using Guard = Soap.Context.Guard;

    public abstract class MapMessagesToFunctions
    {
        protected MapMessagesToFunctions()
        {
            RegisterOnlyForSampleApi();
        }

        private readonly Dictionary<Type, IMessageFunctionsServerSide> messageMappings =
            new Dictionary<Type, IMessageFunctionsServerSide>();

        protected virtual void RegisterOnlyForSampleApi() {}
        
        public List<Type> Events =>
            this.messageMappings.Where(x => x.Key.InheritsOrImplements(typeof(ApiEvent))).Select(x => x.Key).ToList();
        
        public List<Type> Commands =>
            this.messageMappings.Where(x => x.Key.InheritsOrImplements(typeof(ApiCommand))).Select(x => x.Key).ToList();

        protected void Register<TMessage>(IMessageFunctionsClientSide<TMessage> messageFunctions) where TMessage : ApiMessage
        {
            var messageType = typeof(TMessage);

            Guard.Against(this.messageMappings.ContainsKey(messageType), "Cannot map the same message type twice");

            this.messageMappings.Add(messageType, new MessageFunctionsBridge<TMessage>(messageFunctions));
            
        }

        internal IMessageFunctionsServerSide MapMessage(ApiMessage message)
        {
            var messageType = message.GetType();

            if (message is MessageFailedAllRetries m)
            {
                var typeOfFailedMessage = Type.GetType(m.TypeName);
                return this.messageMappings[typeOfFailedMessage];
            }
            else
            {
                Guard.Against(!this.messageMappings.ContainsKey(messageType),
                    $"Could not find a mapping for message this message { messageType.FullName }");

                return this.messageMappings[messageType];    
            }
        }
    }
}
