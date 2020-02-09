namespace Soap.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using DataStore.Models.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

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

        public static async Task CommitChanges()
        {
            Guard.Against(
                QueuedChanges.Count > 1 && QueuedChanges.Any(x => !IsDurableChange(x)),
                "You cannot queue durable and non-durable changes in the same unit of work. If a unit of work contains a non-durable"
                + " change it must be the only change in the unit of work. i.e. you must send non-durable changes to their own unit-of-work"
                + " using a new message. This is ensure the UoW can be persisted in case of failure. All other I/O bound ops must be pushed"
                + " to the perimeter");

            /* from this point on we can crash, throw, lose power, it won't matter all
            will be continued when the message is next dequeued*/
            await SaveUnitOfWork();
            
            await MContext.DataStore.CommitChanges();
            await MContext.BusContext.CommitChanges();
            await MContext.AfterMessageLogEntryObtained.MessageLogEntry.CompleteUnitOfWork();

            /* any other arbitrary calls made e.g. to 3rd party API etc. */
            foreach (var queuedStateChange in QueuedChanges)
            {
                await queuedStateChange.CommitClosure();
                queuedStateChange.Committed = true;
            }
        }

        public static void QueueChange(IQueuedStateChange queuedStateChange)
        {
            /* any other arbitrary calls made e.g. to 3rd party API etc. */
            MContext.MessageAggregator.Collect(queuedStateChange);
        }

        public static Task SaveUnitOfWork()
        {
            var u = MContext.AfterMessageLogEntryObtained.MessageLogEntry.UnitOfWork;

            foreach (var queuedStateChange in QueuedChanges)
                if (IsDurableChange(queuedStateChange))
                {
                    var soapUnitOfWorkId = MContext.AfterMessageLogEntryObtained.MessageLogEntry.id;

                    switch (queuedStateChange)
                    {
                        case QueuedApiCommand c:
                            u.BusCommandMessages.Add(new BusMessageUnitOfWorkItem(c.Command));
                            break;

                        case QueuedApiEvent e:
                            u.BusEventMessages.Add(new BusMessageUnitOfWorkItem(e.Event));
                            break;

                        case IQueuedDataStoreWriteOperation d1 when d1.Is(typeof(QueuedCreateOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.Create));
                            break;
                        case IQueuedDataStoreWriteOperation d1 when d1.Is(typeof(QueuedHardDeleteOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.HardDelete));
                            break;
                        case IQueuedDataStoreWriteOperation d1 when d1.Is(typeof(QueuedUpdateOperation<>)):
                            u.DataStoreCreateOperations.Add(
                                new DataStoreUnitOfWorkItem(
                                    d1.PreviousModel,
                                    d1.NewModel,
                                    soapUnitOfWorkId,
                                    DataStoreUnitOfWorkItem.OperationTypes.Update));
                            break;
                    }
                }

            return MContext.AfterMessageLogEntryObtained.MessageLogEntry.UpdateUnitOfWork(u);
            /* from this point on we can crash, throw, lose power, it won't matter all
            will be continued when the message is next dequeued*/
        }

        private static bool IsDurableChange(IQueuedStateChange x)
        {
            return x is IQueuedDataStoreWriteOperation || x is IQueuedBusMessage;
        }
    }
}