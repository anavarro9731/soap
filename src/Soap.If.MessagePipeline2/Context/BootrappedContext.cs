namespace Soap.MessagePipeline.Context
{
    using System;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class BootrappedContext
    {
        public IMapMessagesToFunctions MessageMapper { get; }

        public readonly ApplicationConfig AppConfig;

        public readonly IAuthenticateUsers Authenticator;

        public readonly IBusContext BusContext;

        public readonly IDataStore DataStore;

        public readonly ILogger Logger;

        public readonly IMessageAggregator MessageAggregator;

        public BootrappedContext(
            IAuthenticateUsers authenticator,
            ApplicationConfig appConfig,
            IDataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBusContext busContext,
            IMapMessagesToFunctions messageMapper)
        {
            MessageMapper = messageMapper;
            this.Authenticator = authenticator;
            this.AppConfig = appConfig;
            this.DataStore = dataStore;
            this.MessageAggregator = messageAggregator;
            this.Logger = logger;
            this.BusContext = busContext;
        }

        protected BootrappedContext(BootrappedContext c)
        {
            this.Authenticator = c.Authenticator;
            this.AppConfig = c.AppConfig;
            this.DataStore = c.DataStore;
            this.MessageAggregator = c.MessageAggregator;
            this.Logger = c.Logger;
            this.BusContext = c.BusContext;
        }
    }

    public static class BootstrapContextExtensions
    {
        public static ContextAfterMessageObtained Upgrade(
            this BootrappedContext current,
            ApiMessage message,
            (DateTime receivedTime, long receivedTicks) timeStamp)
        {
            return new ContextAfterMessageObtained(message, timeStamp, current);
        }
    }
}