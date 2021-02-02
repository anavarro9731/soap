﻿namespace Soap.Context.Context
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore;
    using DataStore.Interfaces.LowLevel;
    using Serilog;
    using Soap.Context.BlobStorage;
    using Soap.Context.MessageMapping;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;

    public class BoostrappedContext
    {
        public readonly IBootstrapVariables AppConfig;

        public readonly IBlobStorage BlobStorage;

        public readonly IBus Bus;

        public readonly DataStore DataStore;

        public readonly ILogger Logger;

        public readonly IMessageAggregator MessageAggregator;

        public readonly MapMessagesToFunctions MessageMapper;

        public readonly NotificationServer NotificationServer;
        
        public BoostrappedContext(
            IBootstrapVariables appConfig,
            DataStore dataStore,
            IMessageAggregator messageAggregator,
            ILogger logger,
            IBus bus,
            NotificationServer notificationServer,
            BlobStorage blobStorage,
            MapMessagesToFunctions messageMapper)
        {
            this.MessageMapper = messageMapper;
            this.AppConfig = appConfig;
            this.DataStore = dataStore;
            this.MessageAggregator = messageAggregator;
            this.Logger = logger;
            this.Bus = bus;
            this.NotificationServer = notificationServer;
            this.BlobStorage = blobStorage;
        }

        protected BoostrappedContext(BoostrappedContext c)
        {
            this.AppConfig = c.AppConfig;
            this.DataStore = c.DataStore;
            this.MessageAggregator = c.MessageAggregator;
            this.Logger = c.Logger;
            this.Bus = c.Bus;
            this.NotificationServer = c.NotificationServer;
            this.MessageMapper = c.MessageMapper;
            this.BlobStorage = c.BlobStorage;
        }
    }

    public static class BootstrapContextExtensions
    {
        public static ContextWithMessage Upgrade(
            this BoostrappedContext current,
            ApiMessage message) =>
            new ContextWithMessage(message, current);
    }
}
