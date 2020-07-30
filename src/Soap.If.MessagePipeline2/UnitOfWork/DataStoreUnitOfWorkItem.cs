namespace Soap.MessagePipeline.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;
    using Soap.Utility.Enums;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Models;

    public class DataStoreUnitOfWorkItem
    {
        public DataStoreUnitOfWorkItem(
            IAggregate beforeModel,
            IAggregate afterModel,
            Guid soapUnitOfWorkId,
            OperationTypes operationType)
        {
            if (beforeModel != null && afterModel != null)
            {
                Guard.Against(beforeModel.id != afterModel.id, "Model mismatch"); //* should never happen
            }

            OperationType = operationType;
            BeforeModel = beforeModel?.ToSerialisableObject();
            AfterModel = afterModel?.ToSerialisableObject();
            UnitOfWorkAfterSuccessfulCommit = soapUnitOfWorkId.ToString(); //* this will be the id if the object commits
            ObjectId = afterModel?.id ?? beforeModel.id; //* consider delete and create scenarios where only one side is populated
            //* this will be the id if the object has not yet committed
            UnitOfWorkBeforeSuccessfulUpdate = beforeModel?.VersionHistory.FirstOrDefault()?.UnitOfWorkId;
        }

        [JsonConstructor]
        private DataStoreUnitOfWorkItem()
        {
        }

        public enum OperationTypes
        {
            Create,

            Update,

            HardDelete
        }

        [JsonProperty]
        public SerialisableObject AfterModel { get; internal set; }

        [JsonProperty]
        public SerialisableObject BeforeModel { get; internal set; }

        [JsonProperty]
        public Guid ObjectId { get; internal set; }

        [JsonProperty]
        public OperationTypes OperationType { get; internal set; }

        [JsonProperty]
        public string UnitOfWorkAfterSuccessfulCommit { get; internal set; }
        
        [JsonProperty]
        public string UnitOfWorkBeforeSuccessfulUpdate { get; internal set; }
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
        public static async Task<(RecordState state, bool superseded)> GetRecordState(
            this DataStoreUnitOfWorkItem item,
            IDataStore dataStore)
        {
            {
                List<Aggregate.AggregateVersionInfo> history = null;

                await GetAggregateHistory(
                    item.ObjectId,
                    (item.BeforeModel ?? item.AfterModel).TypeName,
                    dataStore,
                    v => history = v);

                return history switch
                {
                    var h when ChangeHasNotBeenCommittedOrWasRolledBack(h) => (RecordState.NotCommittedOrRolledBack, HasChangeBeenSupersededForUncommittedRecords(history, item.OperationType)),
                    _ => (RecordState.Committed, HasChangeBeenSupersededForCommittedRecords(history, item.OperationType))
                };
            }

            bool ChangeHasNotBeenCommittedOrWasRolledBack(List<Aggregate.AggregateVersionInfo> history)
            {
                return item.OperationType switch
                {
                    DataStoreUnitOfWorkItem.OperationTypes.Create => !AggregateExists(history),
                    DataStoreUnitOfWorkItem.OperationTypes.HardDelete => AggregateExists(history),
                    DataStoreUnitOfWorkItem.OperationTypes.Update => !history.Exists(v => v.UnitOfWorkId == item.UnitOfWorkAfterSuccessfulCommit),
                    _ => throw new DomainException("Operation Type Unknown")
                };

                bool AggregateExists(List<Aggregate.AggregateVersionInfo> history) => history != null;
            }

            /* dont rely on eTag where, too many dependencies on that feature will make it brittle
             checking the now consistent history is equally as good. etag will still be the ultimate
             arbiter of all subsequent changes during retries. This is simply used to skip rollbacks
             where they have already been superseded. */
            bool HasChangeBeenSupersededForCommittedRecords(
                List<Aggregate.AggregateVersionInfo> history,
                DataStoreUnitOfWorkItem.OperationTypes itemOperationType)
            {
                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Create ||
                    item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update) //* on deleted items there will be no history
                {
                    Guard.Against(
                        !history.Exists(h => h.UnitOfWorkId == item.UnitOfWorkAfterSuccessfulCommit),
                        "Something is wrong record should always exist if the change has been committed",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);

                    var thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork =
                        history.First( /* first means latest */).UnitOfWorkId != item.UnitOfWorkAfterSuccessfulCommit;
                    return thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork;
                }

                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.HardDelete)
                {
                    Guard.Against(
                        itemOperationType != DataStoreUnitOfWorkItem.OperationTypes.HardDelete,
                        "Something is wrong if history is null operation type should always be hard delete",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                    return false;
                }
                throw new ArgumentOutOfRangeException();
            }
            
            /* dont rely on eTag where, too many dependencies on that feature will make it brittle
             checking the now consistent history is equally as good. etag will still be the ultimate
             arbiter of all subsequent changes during retries This is used to skip attempting to commit
            and start rolling back when it's clear the commit will fail due to etag violation as etag
            is saved on the initial attempt and not recalculated */
            bool HasChangeBeenSupersededForUncommittedRecords(
                List<Aggregate.AggregateVersionInfo> history,
                DataStoreUnitOfWorkItem.OperationTypes itemOperationType)
            {
                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.HardDelete) return false; //azure and datastore don't stop deletes with outdated eTags
                
                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update) 
                {
                    var thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork =
                        history.First( /* first means latest */).UnitOfWorkId != item.UnitOfWorkBeforeSuccessfulUpdate;
                    return thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork;   
                }

                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Create)
                {
                    Guard.Against(
                        history == null && itemOperationType != DataStoreUnitOfWorkItem.OperationTypes.Create,
                        "Something is wrong if history is null operation type should always be create",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);
                    return false;
                }
                throw new ArgumentOutOfRangeException();
            }

            async Task GetAggregateHistory(
                Guid aggregateId,
                string typename,
                IDataStore dataStore,
                Action<List<Aggregate.AggregateVersionInfo>> setHistory)
            {
                var readById = typeof(IDataStore).GetMethods()
                                                 .Single(
                                                     m => m.GetGenericArguments().Length == 1
                                                          && m.Name == nameof(DataStore.ReadById))
                                                 .MakeGenericMethod(Type.GetType(typename));

                var result = await readById.InvokeAsync(dataStore, aggregateId, null, null);

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
                DataStoreUnitOfWorkItem.OperationTypes.HardDelete =>
                create(item.BeforeModel.FromSerialisableObject<IAggregate>()),
                _ => throw new DomainException("Unhandled Operation Type")
            };
            return task;
        }
    }
}