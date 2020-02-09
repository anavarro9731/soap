namespace Soap.MessagePipeline.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Models;

    public class DataStoreUnitOfWorkItem
    {
        public DataStoreUnitOfWorkItem(IAggregate beforeModel, IAggregate afterModel, Guid soapUnitOfWorkId, OperationTypes operationType)
        {
            Guard.Against(beforeModel.id != afterModel.id, "Model mismatch"); //- should never happen
            OperationType = operationType;
            BeforeModel = beforeModel.ToSerialisableObject();
            AfterModel = afterModel.ToSerialisableObject();
            UnitOfWorkId = soapUnitOfWorkId.ToString();
            ObjectId = afterModel.id;
        }

        internal DataStoreUnitOfWorkItem()
        {
            //- serialiser
        }

        public enum OperationTypes
        {
            Create,

            Update,

            HardDelete
        }

        public SerialisableObject AfterModel { get; internal set; }

        public SerialisableObject BeforeModel { get; internal set; }

        public Guid ObjectId { get; internal set; }

        public OperationTypes OperationType { get; internal set; }

        public string UnitOfWorkId { get; internal set; }
    }

    public static class DataStoreUnitOfWorkItemExtensions
    {
        [Flags]
        public enum RecordState
        {
            NotCommittedOrRolledBack,

            Committed
        }

        /* used on retries to know the state of a message */
        public static async Task<(RecordState state, bool superseded)> GetRecordState(this DataStoreUnitOfWorkItem item)
        {
            {
                List<Aggregate.AggregateVersionInfo> history = null;

                await GetAggregateHistory(item.ObjectId, (item.BeforeModel ?? item.AfterModel).TypeName, v => history = v);
               
                var superseded = ChangeHasBeenSuperseded(history);

                return history switch
                {
                    var h when ChangeHasNotBeenCommittedOrWasRolledBack(h) => (RecordState.NotCommittedOrRolledBack, superseded),
                    _ => (RecordState.Committed, superseded)
                };
            }

            bool ChangeHasNotBeenCommittedOrWasRolledBack(List<Aggregate.AggregateVersionInfo> history)
            {
                return item.OperationType switch
                {
                    DataStoreUnitOfWorkItem.OperationTypes.Create => !AggregateExists(history),
                    DataStoreUnitOfWorkItem.OperationTypes.HardDelete => AggregateExists(history),
                    DataStoreUnitOfWorkItem.OperationTypes.Update => !history.Exists(v => v.UnitOfWorkId == item.UnitOfWorkId),
                    _ => throw new DomainException("Operation Type Unknown")
                };

                bool AggregateExists(List<Aggregate.AggregateVersionInfo> history)
                {
                    return history != null;
                }
            }

            /* dont rely on eTag where, too many dependencies on that feature will make it brittle
             checking the now consistent history is equally as good. etag will still be the ultimate
             arbiter of all subsequent changes during retries. This is simply used to skip rollbacks
             where they have already been superseded. */
            bool ChangeHasBeenSuperseded(List<Aggregate.AggregateVersionInfo> history)
            {
                return history != null && history.Last().UnitOfWorkId != item.UnitOfWorkId;
            }

            async Task GetAggregateHistory(Guid aggregateId, string typename, Action<List<Aggregate.AggregateVersionInfo>> setHistory)
            {
                var readById = typeof(IDataStore).GetMethod(nameof(DataStore.ReadById)).MakeGenericMethod(Type.GetType(typename));

                var result = await readById.InvokeAsync(MContext.DataStore, aggregateId);

                //- relying on history never being null if the aggregate exists
                setHistory(((IAggregate)result)?.VersionHistory);
            }
        }


        public static Task Rollback(
            this DataStoreUnitOfWorkItem item,
            Func<IAggregate, Task> delete,
            Func<IAggregate, Task> create,
            Func<IAggregate, Task> resetTo)
        {
            var task = item.OperationType switch
            {
                DataStoreUnitOfWorkItem.OperationTypes.Create => delete(item.AfterModel.FromSerialisableObject<IAggregate>()),
                DataStoreUnitOfWorkItem.OperationTypes.Update => resetTo(item.BeforeModel.FromSerialisableObject<IAggregate>()),
                DataStoreUnitOfWorkItem.OperationTypes.HardDelete => create(item.BeforeModel.FromSerialisableObject<IAggregate>()),
                _ => throw new DomainException("Unhandled Operation Type")
            };
            return task;
        }
    }
}