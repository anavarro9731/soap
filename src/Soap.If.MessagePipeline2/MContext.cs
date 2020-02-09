namespace Soap.If.MessagePipeline
{
    using System;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Permissions;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Logging;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.If.Utility.Functions.Extensions;
    using Soap.If.Utility.Functions.Operations;
    using Soap.If.Utility.Objects.Blended;
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

            internal static void UpdateContext(ApiMessage message, MessageLogEntry logEntry,
                (DateTime receivedAt, long ticks) timeStamp, IIdentityWithPermissions identity)
            {
                MContext.AfterMessageLogEntryObtained.Set(
                    new MessageMeta
                    {
                        StartTicks = timeStamp.ticks,
                        ReceivedAt = timeStamp.receivedAt,
                        Schema = message.GetType().AssemblyQualifiedName,
                        RequestedBy = identity
                    },
                    null, logEntry);
            }

            private static void Set(MessageMeta meta, IMapErrorCodesFromDomainToMessageErrorCodes mapper, MessageLogEntry logEntry)
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