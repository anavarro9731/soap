namespace Soap.PfBase.Logic.ProcessesAndOperations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces;
    using Serilog;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer;
    using Soap.PfBase.Logic.ProcessesAndOperations.ProcessMessages;
    using Soap.Utility;
    using Soap.Utility.Functions.Extensions;
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

        protected BusWrapper Bus => new BusWrapper(context.Bus, this.Id, context.Message);

        protected DataStoreReadOnly DataReader => context.DataStore.AsReadOnly();

        protected ILogger Logger => context.Logger;

        protected NotificationServer NotificationServer => context.NotificationServer;

        protected dynamic References => this.processState.References;

        private ContextWithMessageLogEntry context => ContextWithMessageLogEntry.Current;

        async Task IBeginStatefulProcess.BeginProcess<TMessage>(TMessage message)
        {
            var process = this as IBeginProcess<TMessage>;

            Guard.Against(process == null, $"Process {GetType().Name} lacks handler for message {message.GetType().Name}");

            this.processState = await context.DataStore.Create(new ProcessState()).ConfigureAwait(false);

            this.Id = new StatefulProcessId(GetType().ToShortAssemblyTypeName(), this.processState.id);

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

            Guard.Against(this.processState.EnumFlags.HasFlag(BuiltInStates.Completed), "Stateful Process Already Completed");

            RecordContinued(message);

            await process.ContinueProcess(message);

            await context.DataStore.Update(this.processState).ConfigureAwait(false);
        }

        protected void CompleteProcess()
        {
            var username = context.MessageLogEntry.MessageMeta.UserProfileOrNull?.Auth0Id;
            RecordCompleted(username);
        }

        private void RecordCompleted(string username)
        {
            this.processState.EnumFlags.AddFlag(BuiltInStates.Completed);
            context.MessageAggregator.Collect(new StatefulProcessCompleted(GetType().Name, username, this.processState));
        }

        private void RecordContinued<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            context.MessageAggregator.Collect(
                new StatefulProcessContinued(
                    GetType().Name,
                    context.MessageLogEntry.MessageMeta.UserProfileOrNull?.Auth0Id,
                    this.processState,
                    message.Headers.GetMessageId()));
        }

        private void RecordStarted<TMessage>(TMessage message) where TMessage : ApiMessage
        {
            this.processState.EnumFlags.AddFlag(BuiltInStates.Started);

            context.MessageAggregator.Collect(
                new StatefulProcessStarted(
                    GetType().Name,
                    context.MessageLogEntry.MessageMeta.UserProfileOrNull?.Auth0Id,
                    this.processState,
                    message.Headers.GetMessageId()));
        }

        protected class BusWrapper
        {
            private readonly IBus bus;

            private readonly StatefulProcessId id;

            private readonly ApiMessage contextMessage;

            public BusWrapper(IBus bus, StatefulProcessId id, ApiMessage contextMessage)
            {
                this.bus = bus;
                this.id = id;
                this.contextMessage = contextMessage;
            }

            public Task Publish(ApiEvent publishEvent, IBusClient.EventVisibilityFlags eventVisibility = null)
            {
                publishEvent.Headers.SetStatefulProcessId(this.id);
                return this.bus.Publish(publishEvent, this.contextMessage, eventVisibility);
            }
            public Task Send(ApiCommand sendCommand, bool forceServiceLevelAuthority = false, DateTimeOffset scheduledAt = default)
            {
                if (sendCommand.Headers.GetMessageId() == SpecialIds.ForceServiceLevelAuthorityOnOutgoingMessages)
                {
                    forceServiceLevelAuthority = true;
                }
                sendCommand.Headers.SetStatefulProcessId(this.id);
                return this.bus.Send(sendCommand, this.contextMessage, forceServiceLevelAuthority, scheduledAt);
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
                this.processState.EnumFlags.AddFlag(additionalStatus);

                return this.dataStore.Update(this.processState);
            }

            public IReadOnlyList<TState> GetState<TState>() where TState : IConvertible =>
                this.processState.EnumFlags.AsTypedEnum<TState>();

            public bool HasState<TState>(TState state) where TState : IConvertible => this.processState.EnumFlags.HasFlag(state);

            public Task<ProcessState> RemoveState(Enum statusToRemove)
            {
                this.processState.EnumFlags.RemoveFlag(statusToRemove);
                return this.dataStore.Update(this.processState);
            }

            public Task<ProcessState> ResetState(Enum newStatus)
            {
                this.processState.EnumFlags = new EnumFlags(newStatus);

                return this.dataStore.Update(this.processState);
            }
        }
    }
}
