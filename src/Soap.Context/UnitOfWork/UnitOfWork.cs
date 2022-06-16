namespace Soap.Context.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;
    using Soap.Utility.Functions.Extensions;

    public sealed class UnitOfWork : Entity
    {
        public UnitOfWork(Guid messageId, bool optimisticConcurrency)
        {
            OptimisticConcurrency = optimisticConcurrency;
            id = messageId;
            Created = DateTime.UtcNow;
            CreatedAsMillisecondsEpochTime = Created.ConvertToMillisecondsEpochTime();
            /* its quite important that these collections are not modified anywhere but where the unit of work is created. so you know that after you saved it, the original version is always sufficient
            TODO they really should be read-only collections just not sure how that would serialise, something to come back to */ 
            BusCommandMessages = new List<BusMessageUnitOfWorkItem>();
            BusEventMessages = new List<BusMessageUnitOfWorkItem>();
            DataStoreCreateOperations = new List<DataStoreUnitOfWorkItem>();
            DataStoreDeleteOperations = new List<DataStoreUnitOfWorkItem>();
            DataStoreUpdateOperations = new List<DataStoreUnitOfWorkItem>();
        }

        [JsonConstructor]
        private UnitOfWork()
        {
            //* serialiser
            BusCommandMessages = new List<BusMessageUnitOfWorkItem>();
            BusEventMessages = new List<BusMessageUnitOfWorkItem>();
            DataStoreCreateOperations = new List<DataStoreUnitOfWorkItem>();
            DataStoreDeleteOperations = new List<DataStoreUnitOfWorkItem>();
            DataStoreUpdateOperations = new List<DataStoreUnitOfWorkItem>();
        }

        public enum State
        {
            New,

            AllComplete,

            AllRolledBack
        }

        [JsonProperty]
        public List<BusMessageUnitOfWorkItem> BusCommandMessages { get;  } 

        [JsonProperty]
        public List<BusMessageUnitOfWorkItem> BusEventMessages { get;  } 

        [JsonProperty]
        public List<DataStoreUnitOfWorkItem> DataStoreCreateOperations { get; } 

        [JsonProperty]
        public List<DataStoreUnitOfWorkItem> DataStoreDeleteOperations { get; } 

        [JsonProperty]
        public List<DataStoreUnitOfWorkItem> DataStoreUpdateOperations { get; } 

        [JsonProperty]
        public bool OptimisticConcurrency { get; private set; } //* NO IDEA WHY, but JSON.NET won't serialise this property without a setter, even though it will populate the collections that also are without setters ¯\_(ツ)_/¯
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