namespace Soap.MessagePipeline
{
    using System;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Permissions;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.BusContext;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public static class MContext
    {
        public class PerCallStageOneVariables
        {
            public PerCallStageOneVariables(IAuthenticateUsers authenticator, ApplicationConfig appConfig, IDataStore dataStore, IMessageAggregator messageAggregator, ILogger logger, IBusContext busContext)
            {
                this.Authenticator = authenticator;
                this.AppConfig = appConfig;
                this.DataStore = dataStore;
                this.MessageAggregator = messageAggregator;
                this.Logger = logger;
                this.BusContext = busContext;
            }

            public readonly IAuthenticateUsers Authenticator;

            public readonly ApplicationConfig AppConfig;

            public readonly IDataStore DataStore;

            public readonly IMessageAggregator MessageAggregator;

            public readonly ILogger Logger;

            public readonly IBusContext BusContext;
        }

        private static bool AfterMessageLogEntrySet => (bool)CallContext.GetData(nameof(AfterMessageLogEntrySet));

        public static ApplicationConfig AppConfig => CallContext.GetData(nameof(ApplicationConfig)).As<ApplicationConfig>();

        public static MessageBus BusContext => CallContext.GetData(nameof(IBusContext)).As<MessageBus>();
        
        public static IDataStore DataStore => CallContext.GetData(nameof(IDataStore)).As<IDataStore>();
        
        public static ILogger Logger => CallContext.GetData(nameof(ILogger)).As<ILogger>();

        public static IMessageAggregator MessageAggregator => CallContext.GetData(nameof(IMessageAggregator)).As<IMessageAggregator>();

        public static Func<ApiMessage, IIdentityWithPermissions> Authenticate =>
            CallContext.GetData(nameof(IAuthenticateUsers)).As<IAuthenticateUsers>().Authenticate;

        internal static void SetForCall(PerCallStageOneVariables vars)
        {
            CallContext.SetData(nameof(ApplicationConfig), vars.AppConfig);
            CallContext.SetData(nameof(IDataStore), vars.DataStore);
            CallContext.SetData(nameof(IMessageAggregator), vars.MessageAggregator);
            CallContext.SetData(nameof(IBusContext), vars.BusContext);
            CallContext.SetData(nameof(ILogger), vars.Logger);
            CallContext.SetData(nameof(IAuthenticateUsers), vars.Authenticator);
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
                MContext.AfterMessageLogEntryObtained.SetForCall(
                    new MessageMeta
                    {
                        StartTicks = timeStamp.ticks,
                        ReceivedAt = timeStamp.receivedAt,
                        Schema = message.GetType().AssemblyQualifiedName,
                        RequestedBy = identity
                    },
                    null, logEntry);
            }

            private static void SetForCall(MessageMeta meta, IMapErrorCodesFromDomainToMessageErrorCodes mapper, MessageLogEntry logEntry)
            {
                CallContext.SetData(nameof(MessageMeta), meta);
                CallContext.SetData(nameof(MessageLogEntry), logEntry);
                CallContext.SetData(nameof(DomainToMessageErrorCodesMapper), mapper);
                CallContext.SetData(nameof(AfterMessageLogEntrySet), true);
            }

            private static void GuardTiming()
            {
                Guard.Against(
                    AfterMessageLogEntrySet == false,
                    $"Cannot access {nameof(AfterMessageLogEntryObtained)} properties until message entry has been created");
            }
        }
    }
}