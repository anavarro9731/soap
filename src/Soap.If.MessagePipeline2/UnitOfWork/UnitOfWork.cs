namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Utility.Models;

    public class UnitOfWork : Aggregate
    {
        public List<CommandBusMessage> BusCommandMessages { get; set; }

        public List<EventBusMessage> BusEventMessages { get; set; }

        public List<DataStoreCreateOperation> DataStoreCreateOperations { get; set; }

        public List<DataStoreDeleteOperation> DataStoreDeleteOperations { get; set; }

        public List<DataStoreUpdateOperation> DataStoreUpdateOperations { get; set; }

        public class CommandBusMessage : SerialisableObject
        {
            DataStore.
        }

        public class DataStoreCreateOperation : SerialisableObject
        {
        }

        public class DataStoreDeleteOperation : SerialisableObject
        {
        }

        public class DataStoreUpdateOperation : SerialisableObject
        {
        }

        public class EventBusMessage : SerialisableObject
        {
        }

    }
}