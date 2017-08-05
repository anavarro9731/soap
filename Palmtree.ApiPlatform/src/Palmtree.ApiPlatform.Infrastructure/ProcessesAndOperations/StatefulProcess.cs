namespace Palmtree.ApiPlatform.Infrastructure.ProcessesAndOperations
{
    using System;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Serilog;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using Palmtree.ApiPlatform.Infrastructure.Messages.ProcessMessages;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Interfaces;
    using Palmtree.ApiPlatform.Utility;
    using Palmtree.ApiPlatform.Utility.PureFunctions;

    public abstract class StatefulProcess<T> : StatefulProcess, IStatefulProcess<T>
    {
        //used to support accessing specific derived type by base class
    }

    public abstract class StatefulProcess : ApiMessageContext
    {
        private ProcessState processState;

        protected Guid ProcessId => this.processState.id;

        protected dynamic References => this.processState.References;

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

        public new void SetDependencies(IDataStore dataStore, IUnitOfWork uow, ILogger logger, IMessageAggregator messageAggregator)
        {
            base.SetDependencies(dataStore, uow, logger, messageAggregator);
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

        protected System.Collections.Generic.IReadOnlyList<TState> GetState<TState>() where TState : IConvertible
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
