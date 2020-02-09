namespace Soap.If.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.If.MessagePipeline.ProcessesAndOperations.ProcessMessages;
    using Soap.If.MessagePipeline.UnitOfWork;
    using Soap.If.Utility;
    using Soap.If.Utility.Functions.Operations;
    using Soap.If.Utility.Objects.Binary;
    using Soap.Pf.BusContext;

    public abstract class StatefulProcess<T> : StatefulProcess, IStatefulProcess<T>
    {
        /* used to support accessing specific derived type from container by interface
         while maintaining access to underlying functionality */
    }

    public abstract class StatefulProcess
    {
        private ProcessState processState;

        protected IDataStoreQueryCapabilities DataReader => MContext.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => MContext.DataStore.WithoutEventReplay;

        protected ILogger Logger => MContext.Logger;

        protected IMessageAggregator MessageAggregator => MContext.MessageAggregator;

        protected Guid ProcessId => this.processState.id;

        protected dynamic References => this.processState.References;

        protected MessageBus MessageBus { get; private set; }

        public async Task BeginProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await MContext.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.StatefulProcessId = ProcessId;

            RecordStarted(message, meta);

            await process.BeginProcess(message, meta).ConfigureAwait(false);

            await MContext.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await MContext.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.StatefulProcessId = ProcessId;

            RecordStarted(message, meta);

            var result = await process.BeginProcess(message, meta);

            await MContext.DataStore.Update(this.processState).ConfigureAwait(false);

            return result;
        }

        public async Task ContinueProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IContinueProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.StatefulProcessId.HasValue, "Message does not have correlation Id");

            this.processState = await MContext.DataStore.ReadActiveById<ProcessState>(message.StatefulProcessId.Value).ConfigureAwait(false);

            RecordContinued(message, meta);

            await process.ContinueProcess(message, meta);

            await MContext.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task<TReturnType> ContinueProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IContinueProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.StatefulProcessId.HasValue, "Message does not have correlation Id");

            this.processState = await MContext.DataStore.ReadActiveById<ProcessState>(message.StatefulProcessId.Value).ConfigureAwait(false);

            RecordContinued(message, meta);

            var result = await process.ContinueProcess(message, meta);

            await MContext.DataStore.Update(this.processState).ConfigureAwait(false);

            return result;
        }

        protected Task<ProcessState> AddState<T>(T additionalStatus) where T : Enum
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            if (this.processState.Flags != null)
            {
                this.processState.Flags.AddState(additionalStatus);
            }
            else
            {
                this.processState.Flags = new Flags(additionalStatus);
            }

            return MContext.DataStore.Update(this.processState);
        }

        protected void CompleteProcess(ApiCommand command, MessageMeta meta)
        {
            var username = meta.RequestedBy?.UserName;
            RecordCompleted(username);
        }

        protected IReadOnlyList<TState> GetState<TState>() where TState : IConvertible
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            return this.processState.Flags.StatesAsT<TState>();
        }

        protected bool HasState<TState>(TState state) where TState : IConvertible
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            return this.processState.Flags.HasState(state);
        }

        protected Task<ProcessState> RemoveState(Enum statusToRemove)
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

            return MContext.DataStore.Update(this.processState);
        }

        protected Task<ProcessState> ReplaceState(Enum newStatus)
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            if (this.processState.Flags != null) this.processState.Flags = new Flags(newStatus);

            return MContext.DataStore.Update(this.processState);
        }

        private void RecordCompleted(string username)
        {
            MessageAggregator.Collect(new StatefulProcessCompleted(GetType().Name, username, this.processState));
        }

        private void RecordContinued<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            MessageAggregator.Collect(new StatefulProcessContinued(GetType().Name, meta.RequestedBy?.UserName, this.processState));
        }

        private void RecordStarted<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            MessageAggregator.Collect(new StatefulProcessStarted(GetType().Name, meta.RequestedBy?.UserName, this.processState));
        }
    }
}