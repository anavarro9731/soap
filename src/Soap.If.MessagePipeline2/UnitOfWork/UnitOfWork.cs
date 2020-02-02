namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Options;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.Models;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    public class UnitOfWork : Aggregate
    {
        public bool OptimisticConcurreny { get; internal set; } = true;

        public List<BusMessageUnitOfWork> BusCommandMessages { get; internal set; }

        public List<BusMessageUnitOfWork> BusEventMessages { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreCreateOperations { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreDeleteOperations { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreUpdateOperations { get; internal set; }

        public async Task<bool> IsComplete()
        {
            foreach (var busMessage in BusCommandMessages)
                if (!await busMessage.IsComplete())
                    return false;

            foreach (var busMessage in BusEventMessages)
                if (!await busMessage.IsComplete())
                    return false;

            foreach (var dataStoreCreateOperation in DataStoreCreateOperations)
                if (!await dataStoreCreateOperation.IsCommitted())
                    return false;

            foreach (var dataStoreUpdateOperation in DataStoreUpdateOperations)
                if (!await dataStoreUpdateOperation.IsCommitted())
                    return false;

            foreach (var datastoreDeleteOperation in DataStoreDeleteOperations)
                if (!await datastoreDeleteOperation.IsCommitted())
                    return false;

            return true;
        }

        public async Task CompleteOrRollback()
        {
            if (!IsComplete())
            {

            }
        }

        public class BusMessageUnitOfWork : SerialisableObject
        {
            public BusMessageUnitOfWork(ApiMessage x)
                : base(x)
            {
                MessageId = x.MessageId;
            }

            public Guid MessageId { get; internal set; }

            /* used on retries to know the state of a message */
            public async Task<bool> IsComplete() => await QueryForCompleteness();

            /* checking completeness by using MessageLogEntry record does not guarantee the
             message has not already been sent as race condition can occur,
             however MessageLog constraints will filter any duplicates solving that situation 
             and for our purposes of knowing whether to resend it is sufficiently consistent
             and will avoid duplicates in 99% of cases */
            private async Task<bool> QueryForCompleteness()
            {
                var logEntry = (await MContext.DataStore.Read<MessageLogEntry>(x => x.id == MessageId)).SingleOrDefault();
                return logEntry != null;
            }
        }

        public class DataStoreUnitOfWork
        {
            public OperationTypes OperationType { get; internal set; }

            public SerialisableObject BeforeModel { get; internal set; }

            public SerialisableObject AfterModel { get; internal set; }

            public string UnitOfWorkId { get; internal set; }

            public Guid ObjectId { get; internal set; }

            public enum OperationTypes
            {
                Create,
                Update,
                HardDelete
            }

            [Flags]
            public enum RecordState
            {
                NotCommittedOrRolledBack,
                CommittedAndSuperseded,
                CommittedAndRemainsLatestVersion
            }

            public DataStoreUnitOfWork(IAggregate beforeModel, IAggregate afterModel, Guid soapUnitOfWorkId, OperationTypes operationType)
            {
                Guard.Against(beforeModel.id != afterModel.id, "Model mismatch"); //- should never happen
                OperationType = operationType;
                BeforeModel = beforeModel.ToSerialisableObject();
                AfterModel = afterModel.ToSerialisableObject();
                UnitOfWorkId = soapUnitOfWorkId.ToString();
                ObjectId = afterModel.id;
            }

            internal DataStoreUnitOfWork()
            { }

            public Task Rollback(Func<IAggregate, Task> delete, Func<IAggregate, Task> create, Func<IAggregate, Task> resetTo)
            {
                Task task = OperationType switch
                {
                    OperationTypes.Create => delete(AfterModel.FromSerialisableObject<IAggregate>()),
                    OperationTypes.Update => resetTo(BeforeModel.FromSerialisableObject<IAggregate>()),
                    OperationTypes.HardDelete => create(BeforeModel.FromSerialisableObject<IAggregate>()),
                    _ => throw new DomainException("Unhandled Operation Type")
                };
                return task;
            }

            public async Task<bool> IsCommitted() =>
                await GetRecordState() == (RecordState.CommittedAndSuperseded | RecordState.CommittedAndRemainsLatestVersion);

            /* used on retries to know the state of a message */
            private async Task<RecordState> GetRecordState()
            {
                {
                    List<AggregateVersionInfo> history = null;

                    await GetAggregateHistory(ObjectId, (BeforeModel ?? AfterModel).TypeName, v => history = v);

                    return history switch
                    {
                        var h when ChangeHasNotBeenCommittedOrWasRolledBack(h) => RecordState.NotCommittedOrRolledBack,
                        var h when ChangeHasBeenCommittedButSuperseded(h) => RecordState.CommittedAndSuperseded,
                        _ => RecordState.CommittedAndRemainsLatestVersion
                    };
                }

                bool AggregateExists(List<AggregateVersionInfo> history) => history != null;

                bool ChangeHasNotBeenCommittedOrWasRolledBack(List<AggregateVersionInfo> history)
                {
                    return this.OperationType switch
                    {
                        OperationTypes.Create => !AggregateExists(history),
                        OperationTypes.HardDelete => AggregateExists((history)),
                        OperationTypes.Update => !history.Exists(v => v.UnitOfWorkId == this.UnitOfWorkId),
                        _ => throw new DomainException("Operation Type Unknown")
                    };
                }

                /* dont rely on eTag where, too many dependencies on that feature will make it brittle
                 checking the now consistent history is equally as good. etag will still be the ultimate
                 arbiter of all subsequent changes during retries. This is simply used to skip rollbacks
                 where they have already been superseded. */
                bool ChangeHasBeenCommittedButSuperseded(List<AggregateVersionInfo> history)
                {
                    if (ChangeHasNotBeenCommittedOrWasRolledBack(history)) return false;

                    return this.OperationType switch
                    {
                        var value when value == OperationTypes.Create || value == OperationTypes.Update
                        => history.Last().UnitOfWorkId != this.UnitOfWorkId,
                        _ => throw new DomainException("Operation Type Unknown")
                    };
                }

                async Task GetAggregateHistory(Guid aggregateId, string typename, Action<List<AggregateVersionInfo>> setHistory)
                {
                    MethodInfo readById = typeof(IDataStore).GetMethod(nameof(DataStore.ReadById)).MakeGenericMethod(Type.GetType(typename));

                    var result = await readById.InvokeAsync(
                        MContext.DataStore,
                        new object[]
                        {
                            aggregateId
                        });

                    //- relying on history never being null if the aggregate exists
                    setHistory(((IAggregate)result)?.VersionHistory);
                }

            }
        }
    }
}