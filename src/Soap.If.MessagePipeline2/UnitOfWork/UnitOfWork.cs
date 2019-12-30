namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces.LowLevel;
    using Soap.If.Utility.Models;

    public class UnitOfWork : Aggregate
    {
        public List<SerialisableObject> BusCommandMessages { get; set; }

        public List<SerialisableObject> BusEventMessages { get; set; }

        //TODO: replace with calls to check aggregate history, its slower but removes consistency problem between complete and aggregate
        public bool Complete => true;
            //BusCommandMessages.All(a => a.Complete) && BusEventMessages.All(a => a.Complete) && DataStoreCreateOperations.All(a => a.Complete)
            //&& DataStoreDeleteOperations.All(a => a.Complete) && DataStoreUpdateOperations.All(a => a.Complete);

        public List<SerialisableObject> DataStoreCreateOperations { get; set; }

        public List<SerialisableObject> DataStoreDeleteOperations { get; set; }

        public List<SerialisableObject> DataStoreUpdateOperations { get; set; }

    }
}