namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Messages.ProcessMessages;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.Models.Aggregates;
    using Soap.Utility;
    using Soap.Utility.PureFunctions;

    public abstract class StatefulProcess<T> : StatefulProcess, IStatefulProcess<T>
    {
        //used to support accessing specific derived type by base class
    }

    public abstract class StatefulProcess
    {
        private ProcessState processState;

        protected IDataStoreQueryCapabilities DataStoreReadOnly { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IMessageAggregator MessageAggregator { get; private set; }

        protected Guid ProcessId => this.processState.id;

        protected dynamic References => this.processState.References;

        protected IUnitOfWork UnitOfWork { get; private set; }

        private IDataStore DataStore { get; set; }

        public async Task BeginProcess<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.StatefulProcessId = ProcessId;

            RecordStarted(message, meta);

            await process.BeginProcess(message, meta).ConfigureAwait(false);

            await DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            var process = this as IBeginProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.StatefulProcessId = ProcessId;

            RecordStarted(message, meta);

            var result = await process.BeginProcess(message, meta);

            await DataStore.Update(this.processState).ConfigureAwait(false);

            return result;
        }

        public async Task ContinueProcess<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            var process = this as IContinueProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.StatefulProcessId.HasValue, "Message does not have correlation Id");

            this.processState = await DataStore.ReadActiveById<ProcessState>(message.StatefulProcessId.Value).ConfigureAwait(false);

            RecordContinued(message, meta);

            await process.ContinueProcess(message, meta);

            await DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task<TReturnType> ContinueProcess<TMessage, TReturnType>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            var process = this as IContinueProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.StatefulProcessId.HasValue, "Message does not have correlation Id");

            this.processState = await DataStore.ReadActiveById<ProcessState>(message.StatefulProcessId.Value).ConfigureAwait(false);

            RecordContinued(message, meta);

            var result = await process.ContinueProcess(message, meta);

            await DataStore.Update(this.processState).ConfigureAwait(false);

            return result;
        }

        public void SetDependencies(IDataStore dataStore, IUnitOfWork uow, ILogger logger, IMessageAggregator messageAggregator)
        {
            UnitOfWork = uow;
            Logger = logger;
            MessageAggregator = messageAggregator;
            DataStore = dataStore;
            DataStoreReadOnly = dataStore.AsReadOnly();
        }

        protected async Task AddState(Enum additionalStatus)
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            if (this.processState.Flags != null)
            {
                this.processState.Flags.AddState(additionalStatus);
            }

            else
            {
                this.processState.Flags = FlaggedState.Create(additionalStatus);
            }

            await DataStore.Update(this.processState).ConfigureAwait(false);
        }

        protected void CompleteProcess(IApiCommand command, ApiMessageMeta meta)
        {
            var username = meta.RequestedBy?.UserName;
            RecordCompleted(username);
        }

        protected IReadOnlyList<TState> GetState<TState>() where TState : IConvertible
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            return this.processState.Flags.StatesAs<TState>();
        }

        protected bool HasState<TState>(TState state) where TState : IConvertible
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            return this.processState.Flags.HasState(state);
        }

        protected async Task RemoveState(Enum statusToRemove)
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            if (this.processState.Flags != null)
            {
                this.processState.Flags.RemoveState(statusToRemove);
            }

            else
            {
                throw new Exception("This saga's state has not been set.");
            }

            await DataStore.Update(this.processState).ConfigureAwait(false);
        }

        protected async Task ReplaceState(Enum newStatus)
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            if (this.processState.Flags != null) this.processState.Flags = FlaggedState.Create(newStatus);

            await DataStore.Update(this.processState).ConfigureAwait(false);
        }

        private void RecordCompleted(string username)
        {
            MessageAggregator.Collect(new StatefulProcessCompleted(GetType().Name, username));
        }

        private void RecordContinued<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            MessageAggregator.Collect(new StatefulProcessContinued(GetType().Name, meta.RequestedBy?.UserName, this.processState));
        }

        private void RecordStarted<TMessage>(TMessage message, ApiMessageMeta meta) where TMessage : IApiCommand
        {
            MessageAggregator.Collect(new StatefulProcessStarted(GetType().Name, meta.RequestedBy?.UserName, this.processState));
        }
    }
}