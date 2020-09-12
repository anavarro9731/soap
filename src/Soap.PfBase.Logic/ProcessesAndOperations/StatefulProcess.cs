namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Binary;

    public abstract class StatefulProcess : IStatefulProcess
    {
        private StatefulProcessId Id;

        private ProcessState processState;

        public enum BuiltInStates
        {
            Started = 18493,

            Completed = 95839
        }

        public StateClass State => new StateClass(this.processState, context.DataStore);

        protected BusWrapper Bus => new BusWrapper(context.Bus, this.Id);

        protected DataStoreReadOnly DataReader => context.DataStore.AsReadOnly();

        protected IWithoutEventReplay DirectDataReader => context.DataStore.WithoutEventReplay;

        protected ILogger Logger => context.Logger;

        protected NotificationServer NotificationServer => context.NotificationServer;

        protected dynamic References => this.processState.References;

        private ContextWithMessageLogEntry context => ContextWithMessageLogEntry.Current;

        async Task IBeginStatefulProcess.BeginProcess<TMessage>(TMessage message)
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await context.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            this.Id = new StatefulProcessId(GetType().AssemblyQualifiedName, this.processState.id);

            message.Headers.SetStatefulProcessId(this.Id); //* keeps it aligned with the rest of the messages in the session 

            RecordStarted(message);

            await process.BeginProcess(message).ConfigureAwait(false);

            await context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        async Task IContinueStatefulProcess.ContinueProcess<TMessage>(TMessage message)
        {
            var process = this as IContinueProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            Guard.Against(!message.Headers.HasStatefulProcessId(), "Message does not have correlation Id");

            this.processState = await context
                                      .DataStore.ReadActiveById<ProcessState>(
                                          message.Headers.GetStatefulProcessId().Value.InstanceId)
                                      .ConfigureAwait(false);

            Guard.Against(this.processState.Flags.HasState(BuiltInStates.Completed), "Stateful Process Already Completed");

            RecordContinued(message);

            await process.ContinueProcess(message);

            await context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        protected void CompleteProcess()
        {
            var username = context.MessageLogEntry.MessageMeta.RequestedBy?.UserName;
            RecordCompleted(username);
        }

        private void RecordCompleted(string username)
        {
            this.processState.Flags.AddState(BuiltInStates.Completed);
            context.MessageAggregator.Collect(new StatefulProcessCompleted(GetType().Name, username, this.processState));
        }

        private void RecordContinued<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            context.MessageAggregator.Collect(
                new StatefulProcessContinued(
                    GetType().Name,
                    context.MessageLogEntry.MessageMeta.RequestedBy?.UserName,
                    this.processState,
                    message.Headers.GetMessageId()));
        }

        private void RecordStarted<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            this.processState.Flags.AddState(BuiltInStates.Started);

            context.MessageAggregator.Collect(
                new StatefulProcessStarted(
                    GetType().Name,
                    context.MessageLogEntry.MessageMeta.RequestedBy?.UserName,
                    this.processState,
                    message.Headers.GetMessageId()));
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