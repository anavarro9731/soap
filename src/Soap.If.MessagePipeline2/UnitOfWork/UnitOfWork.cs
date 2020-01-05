namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.Models;

    public class UnitOfWork : Aggregate
    {
        public List<BusMessageUnitOfWork> BusCommandMessages { get; internal set; }

        public List<BusMessageUnitOfWork> BusEventMessages { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreCreateOperations { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreDeleteOperations { get; internal set; }

        public List<DataStoreUnitOfWork> DataStoreUpdateOperations { get; internal set; }

        public async Task<bool> IsComplete()
        {
            foreach (var busMessage in BusCommandMessages)
                if (!await busMessage.IsComplete())
                {
                    return false;
                }

            foreach (var busMessage in BusEventMessages)
                if (!await busMessage.IsComplete())
                {
                    return false;
                }

            foreach (var dataStoreCreateOperation in DataStoreCreateOperations)
                if (!await dataStoreCreateOperation.IsComplete())
                {
                    return false;
                }

            foreach (var dataStoreUpdateOperation in DataStoreUpdateOperations)
                if (!await dataStoreUpdateOperation.IsComplete())
                {
                    return false;
                }

            foreach (var datastoreDeleteOperation in DataStoreDeleteOperations)
                if (!await datastoreDeleteOperation.IsComplete())
                {
                    return false;
                }

            return true;
        }

        public class BusMessageUnitOfWork : SerialisableObject
        {
            //- cache for perf
            private bool? isComplete = false;

            public BusMessageUnitOfWork(ApiMessage x)
                : base(x)
            {
                MessageId = x.MessageId;
            }

            public Guid MessageId { get; internal set; }

            public async Task<bool> IsComplete()
            {
                return this.isComplete ??= await QueryForCompleteness();
            }

            /* this record does not guarantee the message has not already been sent as race condition can occur,
             however messagelog constraints will filter this out*/
            private async Task<bool> QueryForCompleteness()
            {
                var logEntry = (await MContext.DataStore.Read<MessageLogEntry>(x => x.id == MessageId)).SingleOrDefault();
                return logEntry != null;
            }
        }

        public class DataStoreUnitOfWork : SerialisableObject
        {
            //- cache for perf
            private bool? isComplete = false;

            public DataStoreUnitOfWork(IHaveAUniqueId x)
                : base(x)
            {
                ObjectId = x.id;
            }

            public Guid ObjectId { get; internal set; }

            public async Task<bool> IsComplete()
            {
                return this.isComplete ??= await QueryForCompleteness();
            }

            /* there is a possibility of a race condition here, however as the change history is audited I think we can ignore and
             take compensating action should that ever occur*/
            private async Task<bool> QueryForCompleteness()
            {
                var history = (await MContext.DataStore.Read<AggregateHistory>(x => x.AggregateId == ObjectId)).SingleOrDefault();
                return history != null;
            }
        }
    }
}