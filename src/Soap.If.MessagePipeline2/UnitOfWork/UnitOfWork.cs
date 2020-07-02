namespace Soap.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.Json.Serialization;
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

        public UnitOfWork()         {
            //* serialiser
        }

        [JsonInclude]
        public List<BusMessageUnitOfWorkItem> BusCommandMessages { get; internal set; } = new List<BusMessageUnitOfWorkItem>();

        [JsonInclude]
        public List<BusMessageUnitOfWorkItem> BusEventMessages { get; internal set; } = new List<BusMessageUnitOfWorkItem>();

        [JsonInclude]
        public List<DataStoreUnitOfWorkItem> DataStoreCreateOperations { get; internal set; } = new List<DataStoreUnitOfWorkItem>();

        [JsonInclude]
        public List<DataStoreUnitOfWorkItem> DataStoreDeleteOperations { get; internal set; } = new List<DataStoreUnitOfWorkItem>();

        [JsonInclude]
        public List<DataStoreUnitOfWorkItem> DataStoreUpdateOperations { get; internal set; } = new List<DataStoreUnitOfWorkItem>();
            
        [JsonInclude]
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