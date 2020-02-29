namespace Soap.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces;

    public class UnitOfWork
    {
        public UnitOfWork(bool optimisticConcurrency)
        {
            OptimisticConcurrency = optimisticConcurrency;
        }

        public enum State
        {
            New,

            AllComplete,

            AllRolledBack
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
        public static async IAsyncEnumerable<Record> DataStoreOperationState(this UnitOfWork unitOfWork, IDataStore dataStore)
        {
            {
                foreach (var op in unitOfWork.DataStoreUpdateOperations.Union(
                    unitOfWork.DataStoreCreateOperations.Union(
                        unitOfWork.DataStoreDeleteOperations)))
                {
                    var state = await op.GetRecordState(dataStore);
                    yield return new Record(op, state.state, state.superseded);
                }
            }
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