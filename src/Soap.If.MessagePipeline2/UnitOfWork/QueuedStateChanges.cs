namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.PureFunctions;

    /*
     * Executes queued changes via a closure.
     * With this approach we can support any data access framework easily
     * without requiring polymorphic implementations of an interface(s) for each
     * supported framework.
     *
     * The downside being that these changes can't be rolled back or persisted
     * with the unit of work. 
     */


    public interface IQueuedBusMessage : IQueuedStateChange { }

    public static class QueuedStateChanges
    {
        public static UnitOfWork AddUnitOfWorkForKnownTypesToQueue()
        {
            Guard.Against(QueuedChanges.Count > 1 && 
                          QueuedChanges.Any(x => (x is IQueuedDataStoreWriteOperation || x is IQueuedBusMessage) == false), 
                "You cannot queue durable and non-durable changes in the same unit of work. If a unit of work contains a non-durable" + 
                " change it must be the only change in the unit of work. i.e. you must send non-durable changes to their own unit-of-work" +
                " using a new message");

            var unitOfwork = new UnitOfWork();

            foreach (var queuedStateChange in QueuedChanges)
            {
                unitOfwork.BusM
            }
            
        }

        public static async Task CommitChanges()
        {
            foreach (var queuedStateChange in QueuedChanges)
            {
                await queuedStateChange.CommitClosure();
                queuedStateChange.Committed = true;
            }
        }

        private static List<IQueuedStateChange> QueuedChanges
        {
            get
            {
                var queuedStateChanges = MMessageContext.MessageAggregator.AllMessages.OfType<IQueuedStateChange>().Where(c => !c.Committed).ToList();
                return queuedStateChanges;
            }
        }

        public static void QueueChange(IQueuedStateChange queuedStateChange)
        {
            MMessageContext.MessageAggregator.Collect(queuedStateChange);
        }
    }
}