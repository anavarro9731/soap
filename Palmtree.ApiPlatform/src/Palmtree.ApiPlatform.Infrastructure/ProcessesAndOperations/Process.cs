﻿namespace Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations
{
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Palmtree.ApiPlatform.Infrastructure.Messages.ProcessMessages;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.Utility.PureFunctions;
    using Serilog;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    /// <summary>
    ///     represents a stateless multi-step process which occurs in a single unit of work
    ///     but involves more than one aggregate instance [regardless or type] and/or more than one service [e.g. EmailSender,
    ///     DataStore]
    ///     It records StatefulProcessStarted/Completed events whose purpose is mainly log instrumentation but could be used in
    ///     unit testing as well.
    /// </summary>
    public abstract class Process<T> : Process, IProcess<T>
    {
        //used to support accessing specific derived type by base class
    }

    public abstract class Process
    {
        protected IDataStoreQueryCapabilities DataStoreReadOnly { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IMessageAggregator MessageAggregator { get; private set; }

        protected IUnitOfWork UnitOfWork { get; private set; }

        public async Task BeginProcess<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            RecordStarted(new ProcessStarted(GetType().Name, meta.RequestedBy?.UserName));

            await process.BeginProcess(message, meta);

            RecordCompleted(new ProcessCompleted(GetType().Name, meta.RequestedBy?.UserName));
        }

        public async Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            var process = this as IBeginProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            RecordStarted(new ProcessStarted(GetType().Name, meta.RequestedBy?.UserName));

            var result = await process.BeginProcess(message, meta);

            RecordCompleted(new ProcessCompleted(GetType().Name, meta.RequestedBy?.UserName));

            return result;
        }

        public void SetDependencies(IDataStore dataStore, IUnitOfWork uow, ILogger logger, IMessageAggregator messageAggregator)
        {
            UnitOfWork = uow;
            Logger = logger;
            MessageAggregator = messageAggregator;
            DataStoreReadOnly = dataStore.AsReadOnly();
        }

        private void RecordCompleted(ProcessCompleted processCompleted)
        {
            MessageAggregator.Collect(processCompleted);
        }

        private void RecordStarted(ProcessStarted statefulProcessStarted)
        {
            MessageAggregator.Collect(statefulProcessStarted);
        }
    }
}