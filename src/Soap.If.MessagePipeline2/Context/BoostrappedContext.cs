namespace Soap.MessagePipeline.Context
{
    using System;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;

    public class BoostrappedContext
    {
        public readonly MapMessagesToFunctions MessageMapper;

        public readonly ApplicationConfig AppConfig;

        public readonly IAuthenticateUsers Authenticator;

        public readonly IBus Bus;

        public readonly IDataStore DataStore;

        public readonly ILogger Logger;

        public readonly IMessageAggregator MessageAggregator;

        public BoostrappedContext(
            IAuthenticateUsers authenticator,
            ApplicationConfig appConfig,
            IDataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBus bus,
            MapMessagesToFunctions messageMapper)
        {
            MessageMapper = messageMapper;
            this.Authenticator = authenticator;
            this.AppConfig = appConfig;
            this.DataStore = dataStore;
            this.MessageAggregator = messageAggregator;
            this.Logger = logger;
            this.Bus = bus;
        }

        protected BoostrappedContext(BoostrappedContext c)
        {
            this.Authenticator = c.Authenticator;
            this.AppConfig = c.AppConfig;
            this.DataStore = c.DataStore;
            this.MessageAggregator = c.MessageAggregator;
            this.Logger = c.Logger;
            this.Bus = c.Bus;
        }

        public class ApplicationConfig
        {
            public int NumberOfApiMessageRetries;

            public string ApplicationName;

            public string ApplicationVersion;

            public string DefaultExceptionMessage;

            public string EnvironmentName;

            public bool ReturnExplicitErrorMessages;

        }
    }

    public static class BootstrapContextExtensions
    {
        public static ContextWithMessage Upgrade(
            this BoostrappedContext current,
            ApiMessage message,
            (DateTime receivedTime, long receivedTicks) timeStamp)
        {
            return new ContextWithMessage(message, timeStamp, current);
        }
    }
}