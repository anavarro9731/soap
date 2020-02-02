namespace Soap.If.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using DataStore;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.Messages;
    using Soap.If.MessagePipeline.Messages;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline2.MessagePipeline;
    using Soap.If.Utility.PureFunctions;
    using Soap.If.Utility.PureFunctions.Extensions;

    /*
     * Executes queued changes via a closure.
     * With this approach we can support any data access framework easily
     * without requiring polymorphic implementations of an interface(s) for each
     * supported framework.
     *
     * The downside being that these changes can't be rolled back or persisted
     * with the unit of work. 
     */

    public interface IQueuedBusMessage : IQueuedStateChange
    {
    }

    public static class QueuedStateChanges
    {
        private static List<IQueuedStateChange> QueuedChanges
        {
            get
            {
                var queuedStateChanges = MContext.MessageAggregator.AllMessages.OfType<IQueuedStateChange>().Where(c => !c.Committed).ToList();
                return queuedStateChanges;
            }
        }

        public static async Task SaveUnitOfWork()
        {
            var u = new UnitOfWork();
            foreach (var queuedStateChange in QueuedChanges)
                if (IsDurableChange(queuedStateChange))
                {
                    if (queuedStateChange.GetType().InheritsOrImplements(typeof(QueuedApiCommand)))
                    {
                        u.BusCommandMessages.Add(new UnitOfWork.BusMessageUnitOfWork(((QueuedApiCommand)queuedStateChange).Command));
                    }

                    if (queuedStateChange.GetType().InheritsOrImplements(typeof(QueuedApiEvent)))
                    {
                        u.BusEventMessages.Add(new UnitOfWork.BusMessageUnitOfWork(((QueuedApiEvent)queuedStateChange).Event));
                    }

                    if (queuedStateChange.GetType().InheritsOrImplements(typeof(QueuedCreateOperation<>)))
                    {
                        u.DataStoreCreateOperations.Add(new UnitOfWork.DataStoreUnitOfWork(((IQueuedDataStoreWriteOperation)queuedStateChange).));
                    }

                    if (queuedStateChange.GetType().InheritsOrImplements(typeof(QueuedUpdateOperation<>)))
                    {
                        u.DataStoreUpdateOperations.Add(new UnitOfWork.DataStoreUnitOfWork(((IQueuedDataStoreWriteOperation)queuedStateChange).Model));
                    }

                    if (queuedStateChange.GetType().InheritsOrImplements(typeof(QueuedCreateOperation<>)))
                    {
                        u.DataStoreDeleteOperations.Add(new UnitOfWork.DataStoreUnitOfWork(((IQueuedDataStoreWriteOperation)queuedStateChange).Model));
                    }

                }

            MContext.AfterMessageLogEntryObtained.MessageLogEntry.AddUnitOfWork(u);
            //- update immediately, you would need find a way to get it to be persisted first so use different instance of ds instead
            var tempDataStore = new DataStore(MContext.AppConfig.DatabaseSettings.CreateRepository());
            await tempDataStore.Update(MContext.AfterMessageLogEntryObtained.MessageLogEntry);
            await tempDataStore.CommitChanges();
            //- from this point on we can crash, throw, lose power, it won't matter all will be continued when the message is next dequeued
        }

        public static async Task CommitChanges()
        {
            Guard.Against(
                QueuedChanges.Count > 1 && QueuedChanges.Any(x => !IsDurableChange(x)),
                "You cannot queue durable and non-durable changes in the same unit of work. If a unit of work contains a non-durable"
                + " change it must be the only change in the unit of work. i.e. you must send non-durable changes to their own unit-of-work"
                + " using a new message. This is ensure the UoW can be persisted in case of failure. All other I/O bound ops must be pushed"
                + " to the perimeter");

            await MContext.DataStore.CommitChanges();
            await MContext.Bus.CommitChanges();

            //any other arbitrary calls made e.g. to 3rd party API etc. committed calls should already be skipped
            foreach (var queuedStateChange in QueuedChanges)
            {
                await queuedStateChange.CommitClosure();
                queuedStateChange.Committed = true;
            }
        }

        public static void QueueChange(IQueuedStateChange queuedStateChange)
        {
            MContext.MessageAggregator.Collect(queuedStateChange);
        }

        private static bool IsDurableChange(IQueuedStateChange x)
        {
            return x is IQueuedDataStoreWriteOperation || x is IQueuedBusMessage;
        }
    }
}