namespace Soap.Context.UnitOfWork
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
                    var h when ChangeHasNotBeenCommittedOrWasRolledBack(h) => (RecordState.NotCommittedOrRolledBack,
                                                                                  HasChangeBeenSupersededForUncommittedRecords(
                                                                                      history,
                                                                                      item.OperationType)),
                    _ => (RecordState.Committed, HasChangeBeenSupersededForCommittedRecords(history, item.OperationType))
                };
            }

            bool ChangeHasNotBeenCommittedOrWasRolledBack(List<Aggregate.AggregateVersionInfo> history)
            {
                return item.OperationType switch
                {
                    DataStoreUnitOfWorkItem.OperationTypes.Create => !AggregateExists(history), 
                    /* Whether a create has been applied is calculated by whether it exists.
                     In theory a competing uow could create the same aggregate and it exists when we didn't create it.
                     if the data used by the other uow is the same who cares? (which is what would happen if say a button was
                     pressed in succession) but with different data that is an edge case we are not accounting for although
                     that is a very unlikely scenario. if it did happen we would see it as existing and not create it leaving
                     the other record with the same id but different data to stand as the result.
                     
                     In a race condition another uow that has already read our new record could also delete the record after 
                     we've created it which would make it appear that it has never been added and prompting us to assume we haven't added it yet.
                     If it doesn't exist we really have no way of knowing however someone deleting something so quickly after it appeared 
                     due to our uow is really an edge. If this did happen our uow will be seen with a partiallycompletebutcannotcomplete 
                     status and it will goto rolling everything back which has committed and it wont attempt the create then because 
                     it will think it has not happened yet and while the uow will be rolled back it should leave things consistent */
                    
                    DataStoreUnitOfWorkItem.OperationTypes.HardDelete => AggregateExists(history),
                    /* we calculate this based on whether the aggregate exists, if it does we say it hasn't been deleted yet.
                     that should be pretty sound all around. another uow could be planning to remove it as well but double delete
                     attempts don't really hurt anything consistency wise */
                    
                    DataStoreUnitOfWorkItem.OperationTypes.Update => AggregateExists(history) 
                    /* we calculate this based on the aggregate existing (another uow could delete it from under us) and that our uowid has been applied
                     to its history. if it doesnt exist in the context of an update that can only mean it did exist and was ard deleted by another uow 
                     if it exists and our uowid doesnt exist in its history then either we didn't apply it yet or some other uow rolled back our change
                     to a previous state. in this case like in the create scenario above our uow will be seen with a partiallycompletebutcannotcomplete 
                     status and it will goto rolling everything back which it has committed and it wont attempt the update then because 
                     it will think it has not happened yet and while the uow will be rolled back it should leave things consistent */
                                                                     && !history.Exists(v => v.UnitOfWorkId == item.UnitOfWorkAfterSuccessfulCommit),
                    
                    _ => throw new DomainException("Operation Type Unknown")
                };

                bool AggregateExists(List<Aggregate.AggregateVersionInfo> history) => history != null;
            }

            /* dont rely on eTag for this determination, too many dependencies on that feature will make it brittle
             and it is already one of the most complex parts. Checking the now history (which is consistent with the record itself) is 
             much clearer conceptually and just as good since the only change we committing using this data are rollbacks which
             shouldn' fail because we don't use eTags on updates during rollbacks and creates and deletes don't use them at all. */
            bool HasChangeBeenSupersededForCommittedRecords(
                List<Aggregate.AggregateVersionInfo> history,
                DataStoreUnitOfWorkItem.OperationTypes itemOperationType)
            {
                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Create
                    || item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update)
                {
                    if (history == null)
                    {
                        /* presumably it was hard deleted (or a create was rolled back) by another uow so
                        returning true results in skipping over the create or update during the rollback 
                        leaving everything consistent
                        */
                        return true;
                    }

                    Guard.Against(
                        !history.Exists(h => h.UnitOfWorkId == item.UnitOfWorkAfterSuccessfulCommit),
                        "Something is wrong, a record of our uow should always exist if the change has been committed",
                        ErrorMessageSensitivity.MessageIsSafeForInternalClientsOnly);

                    var thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork =
                        history.First( /* first means latest */).UnitOfWorkId != item.UnitOfWorkAfterSuccessfulCommit;
                    
                    //* if another uow has incremented it return false skipping over it on rollbacks
                    return
                        thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork; 
                }

                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.HardDelete)
                {
                    /* can't really supersede a delete, there are edge cases where another uow could
                     could have rolled back a delete after you committed this delete of that aggregate it 
                     and so now it looks like you didn't commit so you will commit the delete a second time but the 
                     effect is pretty much the same the record is deleted and neither uow is technically
                     inconsistent */
                    return false;
                }

                throw new ArgumentOutOfRangeException();
            }

            /* dont rely on eTag where, too many dependencies on that feature will make it brittle
             checking the now consistent history is equally as good. etag will still be the ultimate
             arbiter of all subsequent changes during retries. This is used to skip attempting to commit
             and start rolling back when it's clear the commit will fail due to etag violation as etag
             is saved on the initial attempt and not recalculated. If eTag violation does occur anyway
             due to race condition it will be picked up by this same function on the next retry. In theory
             if this eTag violation slips by on the last retry you would not have a chance to rollback
             but that is unlikely if you have at least 2-3 retries because it is on the FIRST failure
             that the rollback process starts and rollback operations are not subject to eTag violations. */
            bool HasChangeBeenSupersededForUncommittedRecords(
                List<Aggregate.AggregateVersionInfo> history,
                DataStoreUnitOfWorkItem.OperationTypes itemOperationType)
            {
                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.HardDelete)
                {
                    return false; /* datastore doesn't stop deletes with outdated eTags so there 
                    will be no failure if someone else has changed the record since you queued the delete 
                    your delete will succeed */
                }

                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update)
                {
                    var thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork =
                        history.First( /* first means latest */).UnitOfWorkId != item.UnitOfWorkBeforeSuccessfulUpdate;
                    return thereHasBeenASubsequentChangeMadeByAnotherUnitOfWork;
                    /* if someone else has changed the uow we will then start rolling back our changes
                    because going ahead would make their uow inconsistent. the consistency window is calculated 
                    from the time the change of a record is queued until it has been committed and
                    this survives across retry attempts */
                }

                if (item.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Create)
                {
                    /* you cannot really supersede a create that hasn't been created yet, unless another uow
                     is trying to create an aggregate with the same id. see note for the Create operation
                     in the ChangeHasNotBeenCommittedOrWasRolledBack function for details on this edge case */
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

                //* relying on history never being null if the aggregate exists
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
                DataStoreUnitOfWorkItem.OperationTypes.HardDelete => create(
                    item.BeforeModel.FromSerialisableObject<IAggregate>()),
                _ => throw new DomainException("Unhandled Operation Type")
            };
            return task;
        }
    }
}