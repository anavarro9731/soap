namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.Messages;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Logging;
    using Soap.If.Utility.Functions.Extensions;
    using Soap.If.Utility.Models;

    public class UnitOfWork
    {
        public UnitOfWork(bool optimisticConcurrency)
        {
            OptimisticConcurrency = optimisticConcurrency;
        }

        public List<BusMessageUnitOfWorkItem> BusCommandMessages { get; internal set; }

        public List<BusMessageUnitOfWorkItem> BusEventMessages { get; internal set; }

        public List<DataStoreUnitOfWorkItem> DataStoreCreateOperations { get; internal set; }

        public List<DataStoreUnitOfWorkItem> DataStoreDeleteOperations { get; internal set; }

        public List<DataStoreUnitOfWorkItem> DataStoreUpdateOperations { get; internal set; }

        public bool OptimisticConcurrency { get; internal set; }
    }

    public static class UnitOfWorkExtensions
    {
        public enum State
        {
            New,

            AllComplete,

            AllRolledBack
        }

        public static async Task<State> AttemptToFinishAPreviousAttempt(
            this UnitOfWork unitOfWork,
            IBusContext busContext,
            IDataStore dataStore)
        {
            {
                /* check the ds UoW's look ahead first to see if there are potential conflicts
                 if there are then we can assume that is why we failed last time and we should rollback any remaining items
                 starting with the creates and updates since they are the ones that other people could have seen
                 and finally returning AllRolledBack */

                //- don't recalculate these expensive ops
                var records = await WaitForAllRecords(unitOfWork);

                return unitOfWork switch
                {
                    var u when IsEmpty(u) => State.New,
                    _ => await AttemptCompletion(records, unitOfWork, busContext)
                };
            }

            static async Task<State> AttemptCompletion(List<Record> records, UnitOfWork unitOfWork, IBusContext busContext)
            {
                if (!unitOfWork.OptimisticConcurrency) return await CompleteDataAndMessages();

                return records switch
                {
                    var r when HasNotStartedDataButCannotFinish(r) => State.AllRolledBack,
                    var r when PartiallyCompletedDataButCannotFinish(r) => await RollbackRemaining(),
                    var r when PartiallyCompletedDataAndCanFinish(r) => await CompleteDataAndMessages(),
                    var r when CompletedDataButNotMarkedAsCompleted(r) => await CompleteMessages(),
                    _ => throw new DomainException(
                             "Unaccounted for case in handling failed unit of work"
                             + $" {MContext.AfterMessageLogEntryObtained.MessageLogEntry.id}")
                };

                async Task<State> RollbackRemaining()
                {
                    foreach (var record in records.Where(x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed))
                        await record.UowItem.Rollback(null, null, null);
                    return State.AllRolledBack;
                }

                async Task<State> CompleteDataAndMessages()
                {
                    await SaveUnsavedData(records);
                    return await CompleteMessages();
                }

                async Task<State> CompleteMessages()
                {
                    await SendAnyUnsentMessages(unitOfWork, busContext);
                    await MContext.AfterMessageLogEntryObtained.MessageLogEntry.CompleteUnitOfWork();
                    return State.AllComplete;
                }

                //* failed after rollback was done or before anything was committed (e.g. first item failed concurrency check)
                static bool PartiallyCompletedDataAndCanFinish(List<Record> records)
                {
                    return records.Any(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack
                             && !ThereAreRecordsThatCannotBeCompleted(records));
                }

                //* failed after rollback was done or before anything was committed (e.g. first item failed concurrency check)
                static bool HasNotStartedDataButCannotFinish(List<Record> records)
                {
                    return records.All(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack
                             && MContext.AfterMessageLogEntryObtained.MessageLogEntry.ProcessingComplete == false);
                }

                /* could be from messages or just the complete flag itself, in any case the complete
                method can be called and it will skip over the completed methods */
                static bool CompletedDataButNotMarkedAsCompleted(List<Record> records)
                {
                    return records.All(
                        x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed
                             && MContext.AfterMessageLogEntryObtained.MessageLogEntry.ProcessingComplete == false);
                }

                static bool PartiallyCompletedDataButCannotFinish(List<Record> records)
                {
                    return ThereAreRecordsThatCannotBeCompleted(records) && records.Any(
                               x => x.State == DataStoreUnitOfWorkItemExtensions.RecordState.Committed);
                }

                static bool ThereAreRecordsThatCannotBeCompleted(List<Record> records)
                {
                    return records.Any(
                        r => r.State == DataStoreUnitOfWorkItemExtensions.RecordState.NotCommittedOrRolledBack && r.Superseded);
                }
            }

            static bool IsEmpty(UnitOfWork u)
            {
                return !u.BusCommandMessages.Any() && !u.BusEventMessages.Any() && !u.DataStoreUpdateOperations.Any()
                       && !u.DataStoreDeleteOperations.Any() && !u.DataStoreCreateOperations.Any();
            }

            static async Task SaveUnsavedData(List<Record> records)
            {
                {
                    /* use order least vulnerable to rollbacks,
                 take updates which and prioritize those that are modified recently 
                 then take creates, because deletes are always allowed through both physically
                 and logically (I don't think we care about race condition when deleting) and if a create
                 fails it means less to rollback if the deletes have not been done yet*/

                    var incompleteRecords = records
                                            .Where(
                                                x => x.State == DataStoreUnitOfWorkItemExtensions
                                                                .RecordState.NotCommittedOrRolledBack)
                                            .OrderBy(
                                                x => x.UowItem.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Update)
                                            .ThenBy(x => x.UowItem.OperationType == DataStoreUnitOfWorkItem.OperationTypes.Create)
                                            .ThenByDescending(
                                                x => x.UowItem.BeforeModel?.Deserialise<IAggregate>()
                                                      .ModifiedAsMillisecondsEpochTime)
                                            .ToList();

                    foreach (var incompleteRecord in incompleteRecords) await SaveRecord(incompleteRecord);
                }

                static async Task SaveRecord(Record r)
                {
                    var before = r.UowItem.BeforeModel?.Deserialise<IAggregate>();
                    var after = r.UowItem.AfterModel?.Deserialise<IAggregate>();

                    switch (r.UowItem.OperationType)
                    {
                        case DataStoreUnitOfWorkItem.OperationTypes.Update:
                            await MContext.DataStore.DocumentRepository.UpdateAsync(after, nameof(AttemptToFinishAPreviousAttempt));
                            break;
                        case DataStoreUnitOfWorkItem.OperationTypes.Create:
                            await MContext.DataStore.DocumentRepository.CreateAsync(
                                after,
                                nameof(AttemptToFinishAPreviousAttempt));
                            break;
                        case DataStoreUnitOfWorkItem.OperationTypes.HardDelete:
                            await MContext.DataStore.DocumentRepository.DeleteAsync(
                                before,
                                nameof(AttemptToFinishAPreviousAttempt));
                            break;
                    }

                }
            }

            static async Task SendAnyUnsentMessages(UnitOfWork unitOfWork, IBusContext busContext)
            {
                /* cannot rollback messages, forward only,
            it's not the same risk as data though since there are no concurrency issues
            it's really only infrastructure problems that would stop you here */
                var incompleteCommands = await unitOfWork.BusCommandMessages.WhereAsync(async x => !await x.IsComplete());
                incompleteCommands.Select(x => x.Deserialise<ApiCommand>()).ToList().ForEach(busContext.Send);

                var incompleteEvents = await unitOfWork.BusEventMessages.WhereAsync(async x => !await x.IsComplete());
                incompleteEvents.Select(x => x.Deserialise<ApiEvent>()).ToList().ForEach(busContext.Publish);
            }

            static async Task<List<Record>> WaitForAllRecords(UnitOfWork unitOfWork)
            {
                var records = new List<Record>();
                await foreach (var item in unitOfWork.DataStoreOperationState()) records.Add(item);

                return records;
            }
        }

        public static async IAsyncEnumerable<Record> DataStoreOperationState(this UnitOfWork unitOfWork)
        {
            {
                foreach (var op in unitOfWork.DataStoreUpdateOperations.Union(
                    unitOfWork.DataStoreCreateOperations.Union(
                        unitOfWork.DataStoreDeleteOperations)))
                {
                    var state = await op.GetRecordState();
                    yield return new Record(op, state.state, state.superseded);
                }
            }
        }

        private static async Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            var results = new ConcurrentQueue<T>();
            var tasks = source.Select(
                async x =>
                    {
                    if (await predicate(x))
                    {
                        results.Enqueue(x);
                    }
                    });
            await Task.WhenAll(tasks);
            return results;
        }

        public class Record
        {
            public readonly DataStoreUnitOfWorkItemExtensions.RecordState State;

            public readonly bool Superseded;

            public readonly DataStoreUnitOfWorkItem UowItem;

            public Record(DataStoreUnitOfWorkItem uowItem, DataStoreUnitOfWorkItemExtensions.RecordState state, bool superseded)
            {
                this.UowItem = uowItem;
                this.State = state;
                this.Superseded = superseded;
            }
        }
    }
}