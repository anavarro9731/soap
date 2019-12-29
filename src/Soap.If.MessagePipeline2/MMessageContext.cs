namespace Soap.If.MessagePipeline2.MessagePipeline
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.Utility.PureFunctions.Extensions;

    public static class MMessageContext
    {
        
        public static IApplicationConfig AppConfig => CallContext.GetData(nameof(AppConfig)).As<IApplicationConfig>();

        public static MessageBus Bus => CallContext.GetData(nameof(Bus)).As<MessageBus>();

        public static IDataStore DataStore => CallContext.GetData(nameof(DataStore)).As<IDataStore>();

        public static ILogger Logger => CallContext.GetData(nameof(Logger)).As<ILogger>();

        public static IMessageAggregator MessageAggregator => CallContext.GetData(nameof(MessageAggregator)).As<IMessageAggregator>();

        

        internal static void Set(
            IApplicationConfig appConfig,
            IDataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBusContext busContext)
        {
            CallContext.SetData(nameof(AppConfig), appConfig);
            CallContext.SetData(nameof(DataStore), dataStore);
            CallContext.SetData(nameof(MessageAggregator), messageAggregator);
            CallContext.SetData(nameof(Bus), new MessageBus(busContext));
            CallContext.SetData(nameof(Logger), logger);
        }

        public static class AfterMessageAccepted
        {
            public static MessageMeta MessageMeta => CallContext.GetData(nameof(MessageMeta)).As<MessageMeta>();

            public static IMapErrorCodesFromDomainToMessageErrorCodes DomainToMessageErrorCodesMapper =>
                CallContext.GetData(nameof(DomainToMessageErrorCodesMapper)).As<IMapErrorCodesFromDomainToMessageErrorCodes>();

            internal static void Set(MessageMeta meta, IMapErrorCodesFromDomainToMessageErrorCodes mapper)
            {
                CallContext.SetData(nameof(MessageMeta), meta);
                CallContext.SetData(nameof(DomainToMessageErrorCodesMapper), mapper);
            }
        }

    }
}