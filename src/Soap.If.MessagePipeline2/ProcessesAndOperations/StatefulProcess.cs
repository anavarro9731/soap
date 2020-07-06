namespace Soap.MessagePipeline.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.ProcessesAndOperations.ProcessMessages;
    using Soap.NotificationServer;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Binary;

    public abstract class StatefulProcess : IProcess
    {
        private readonly ContextWithMessageLogEntry context = ContextWithMessageLogEntry.Current;

        private StatefulProcessId Id;

        private ProcessState processState;

        public StateClass State => new StateClass(this.processState, this.context.DataStore);

        protected BusWrapper Bus => new BusWrapper(this.context.Bus, this.Id);

        protected DataStoreReadOnly DataReader => this.context.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => this.context.DataStore.WithoutEventReplay;

        protected ILogger Logger => this.context.Logger;

        protected NotificationServer NotificationServer => this.context.NotificationServer;

        protected dynamic References => this.processState.References;

        public async Task BeginProcess<TMessage>(TMessage message) where TMessage : ApiCommand
        {
            var process = this as IBeginProcess<TMessage>;

            this.Id = new StatefulProcessId(GetType().FullName, this.processState.id);

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await this.context.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            message.Headers.SetStatefulProcessId(this.Id); //* keeps it aligned with the rest of the messages in the session 

            RecordStarted(message);

            await process.BeginProcess(message).ConfigureAwait(false);

            await this.context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        public async Task ContinueProcess<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            var process = this as IContinueProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.Headers.HasStatefulProcessId(), "Message does not have correlation Id");

            this.processState = await this.context.DataStore
                                          .ReadActiveById<ProcessState>(message.Headers.GetStatefulProcessId().InstanceId)
                                          .ConfigureAwait(false);

            RecordContinued(message);

            await process.ContinueProcess(message);

            await this.context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        protected void CompleteProcess(ApiMessage command)
        {
            var username = this.context.MessageLogEntry.MessageMeta.RequestedBy?.UserName;
            RecordCompleted(username);
        }

        private void RecordCompleted(string username)
        {
            this.context.MessageAggregator.Collect(new StatefulProcessCompleted(GetType().Name, username, this.processState));
        }

        private void RecordContinued<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            this.context.MessageAggregator.Collect(
                new StatefulProcessContinued(
                    GetType().Name,
                    this.context.MessageLogEntry.MessageMeta.RequestedBy?.UserName,
                    this.processState));
        }

        private void RecordStarted<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            this.context.MessageAggregator.Collect(
                new StatefulProcessStarted(
                    GetType().Name,
                    this.context.MessageLogEntry.MessageMeta.RequestedBy?.UserName,
                    this.processState));
        }

        public class BusWrapper
        {
            private readonly IBus bus;

            private readonly StatefulProcessId id;

            public BusWrapper(IBus bus, StatefulProcessId id)
            {
                this.bus = bus;
                this.id = id;
            }

            public Task Publish(ApiEvent publishEvent)
            {
                publishEvent.Headers.SetStatefulProcessId(this.id);
                return this.bus.Publish(publishEvent);
            }

            public Task Send(ApiCommand sendCommand)
            {
                sendCommand.Headers.SetStatefulProcessId(this.id);
                return this.bus.Send(sendCommand);
            }
        }

        public class StateClass
        {
            private readonly DataStore dataStore;

            private readonly ProcessState processState;

            public StateClass(ProcessState processState, DataStore dataStore)
            {
                this.processState = processState;
                this.dataStore = dataStore;
            }

            public Task<ProcessState> AddState<T>(T additionalStatus) where T : Enum
            {
                this.processState.Flags.AddState(additionalStatus);

                return this.dataStore.Update(this.processState);
            }

            public IReadOnlyList<TState> GetState<TState>() where TState : IConvertible =>
                this.processState.Flags.StatesAsT<TState>();

            public bool HasState<TState>(TState state) where TState : IConvertible => this.processState.Flags.HasState(state);

            public Task<ProcessState> RemoveState(Enum statusToRemove)
            {
                this.processState.Flags.RemoveState(statusToRemove);
                return this.dataStore.Update(this.processState);
            }

            public Task<ProcessState> ResetState(Enum newStatus)
            {
                this.processState.Flags = new Flags(newStatus);

                return this.dataStore.Update(this.processState);
            }
        }
    }
}