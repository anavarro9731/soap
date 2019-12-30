namespace Soap.If.MessagePipeline2.MessagePipeline
{
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Soap.Pf.BusContext;

    public static class MContext
    {
        private static bool afterMessageLogEntryCreated;

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
            CallContext.SetData(nameof(Bus), busContext);
            CallContext.SetData(nameof(Logger), logger);
        }

        public static class AfterMessageLogEntryObtained
        {
            public static IMapErrorCodesFromDomainToMessageErrorCodes DomainToMessageErrorCodesMapper
            {
                get
                {
                    GuardTiming();
                    return CallContext.GetData(nameof(DomainToMessageErrorCodesMapper)).As<IMapErrorCodesFromDomainToMessageErrorCodes>();
                }
            }

            public static MessageLogEntry MessageLogEntry
            {
                get
                {
                    GuardTiming();
                    return CallContext.GetData(nameof(MessageLogEntry)).As<MessageLogEntry>();
                }
            }

            public static MessageMeta MessageMeta
            {
                get
                {
                    GuardTiming();
                    return CallContext.GetData(nameof(MessageMeta)).As<MessageMeta>();
                }
            }

            internal static void Set(MessageMeta meta, IMapErrorCodesFromDomainToMessageErrorCodes mapper, MessageLogEntry logEntry)
            {
                CallContext.SetData(nameof(MessageMeta), meta);
                CallContext.SetData(nameof(MessageLogEntry), logEntry);
                CallContext.SetData(nameof(DomainToMessageErrorCodesMapper), mapper);
                afterMessageLogEntryCreated = true;
            }

            private static void GuardTiming()
            {
                Guard.Against(
                    afterMessageLogEntryCreated == false,
                    $"Cannot access {nameof(AfterMessageLogEntryObtained)} properties until message entry has been created");
            }
        }
    }
}