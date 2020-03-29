namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Bus;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Binary;

    public abstract class StatefulProcess<T> : StatefulProcess, IStatefulProcess<T>
    {
        protected StatefulProcess(ContextWithMessage context)
            : base(context)
        {
        }

        /* used to support accessing specific derived type from container by interface
         while maintaining access to underlying functionality */
    }

    public abstract class StatefulProcess
    {
        private readonly ContextWithMessage context;

        private ProcessState processState;

        protected StatefulProcess(ContextWithMessage context)
        {
            this.context = context;
        }

        protected IDataStoreQueryCapabilities DataReader => this.context.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        protected ILogger Logger => this.context.Logger;

        protected Bus Bus { get; private set; }

        protected Guid ProcessId => this.processState.id;

        protected dynamic References => this.processState.References;

        public async Task BeginProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await this.context.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.StatefulProcessId = ProcessId;

            RecordStarted(message, meta);

            await process.BeginProcess(message, meta).ConfigureAwait(false);

            await this.context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task<TReturnType> BeginProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta)
            where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await this.context.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.StatefulProcessId = ProcessId;

            RecordStarted(message, meta);

            var result = await process.BeginProcess(message, meta);

            await this.context.DataStore.Update(this.processState).ConfigureAwait(false);

            return result;
        }

        public async Task ContinueProcess<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            var process = this as IContinueProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.StatefulProcessId.HasValue, "Message does not have correlation Id");

            this.processState = await this.context.DataStore.ReadActiveById<ProcessState>(message.StatefulProcessId.Value)
                                          .ConfigureAwait(false);

            RecordContinued(message, meta);

            await process.ContinueProcess(message, meta);

            await this.context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task<TReturnType> ContinueProcess<TMessage, TReturnType>(TMessage message, MessageMeta meta)
            where TMessage : ApiCommand
        {
            var process = this as IContinueProcess<TMessage, TReturnType>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.StatefulProcessId.HasValue, "Message does not have correlation Id");

            this.processState = await this.context.DataStore.ReadActiveById<ProcessState>(message.StatefulProcessId.Value)
                                          .ConfigureAwait(false);

            RecordContinued(message, meta);

            var result = await process.ContinueProcess(message, meta);

            await this.context.DataStore.Update(this.processState).ConfigureAwait(false);

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

            return this.context.DataStore.Update(this.processState);
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

            return this.context.DataStore.Update(this.processState);
        }

        protected Task<ProcessState> ReplaceState(Enum newStatus)
        {
            Guard.Against(this.processState == null, "This saga is not stateful.");

            if (this.processState.Flags != null) this.processState.Flags = new Flags(newStatus);

            return this.context.DataStore.Update(this.processState);
        }

        private void RecordCompleted(string username)
        {
            this.context.MessageAggregator.Collect(new StatefulProcessCompleted(GetType().Name, username, this.processState));
        }

        private void RecordContinued<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            this.context.MessageAggregator.Collect(
                new StatefulProcessContinued(GetType().Name, meta.RequestedBy?.UserName, this.processState));
        }

        private void RecordStarted<TMessage>(TMessage message, MessageMeta meta) where TMessage : ApiCommand
        {
            this.context.MessageAggregator.Collect(
                new StatefulProcessStarted(GetType().Name, meta.RequestedBy?.UserName, this.processState));
        }
    }
}