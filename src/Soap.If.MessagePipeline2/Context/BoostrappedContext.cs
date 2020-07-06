namespace Soap.MessagePipeline.Context
{
    using System;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using Serilog;
    using Soap.Bus;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.NotificationServer;

    public class BoostrappedContext
    {
        public readonly ApplicationConfig AppConfig;

        public readonly IAuthenticateUsers Authenticator;

        public readonly IBus Bus;

        public readonly DataStore DataStore;

        public readonly ILogger Logger;

        public readonly IMessageAggregator MessageAggregator;

        public readonly MapMessagesToFunctions MessageMapper;

        public readonly NotificationServer NotificationServer;

        public BoostrappedContext(
            IAuthenticateUsers authenticator,
            ApplicationConfig appConfig,
            DataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBus bus,
            NotificationServer notificationServer,
            MapMessagesToFunctions messageMapper)
        {
            this.MessageMapper = messageMapper;
            this.Authenticator = authenticator;
            this.AppConfig = appConfig;
            this.DataStore = dataStore;
            this.MessageAggregator = messageAggregator;
            this.Logger = logger;
            this.Bus = bus;
            this.NotificationServer = notificationServer;
        }

        protected BoostrappedContext(BoostrappedContext c)
        {
            this.Authenticator = c.Authenticator;
            this.AppConfig = c.AppConfig;
            this.DataStore = c.DataStore;
            this.MessageAggregator = c.MessageAggregator;
            this.Logger = c.Logger;
            this.Bus = c.Bus;
            this.MessageMapper = c.MessageMapper;
        }

        public class ApplicationConfig
        {
            public string ApplicationName;

            public string ApplicationVersion;

            public string DefaultExceptionMessage;

            public string EnvironmentName;

            public int NumberOfApiMessageRetries;

            public bool ReturnExplicitErrorMessages;
        }
    }

    public static class BootstrapContextExtensions
    {
        public static ContextWithMessage Upgrade(
            this BoostrappedContext current,
            ApiMessage message,
            (DateTime receivedTime, long receivedTicks) timeStamp) =>
            new ContextWithMessage(message, timeStamp, current);
    }
}