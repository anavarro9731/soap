namespace Palmtree.ApiPlatform.MessagePipeline.UnitOfWork
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

    /// <summary>
    ///     Executes queued changes via a closure.
    ///     With this approach we can support any data access framework easily
    ///     without requiring polymorphic implementations of an interface(s) for each
    ///     supported framework.
    /// </summary>
    public class QueuedStateChanger
    {
        private readonly IMessageAggregator messageAggregator;

        public QueuedStateChanger(IMessageAggregator messageAggregator)
        {
            this.messageAggregator = messageAggregator;
        }

        public Guid TransactionId { get; set; }

        public async Task CommitChanges()
        {
            var queuedStateChanges = this.messageAggregator.AllMessages.OfType<IQueuedStateChange>().Where(c => !c.Committed).ToList();
            foreach (var queuedStateChange in queuedStateChanges)
            {
                await queuedStateChange.CommitClosure().ConfigureAwait(false);
                queuedStateChange.Committed = true;
            }
        }

        public void QueueChange(IQueuedStateChange queuedStateChange)
        {
            this.messageAggregator.Collect(queuedStateChange);
        }
    }
}
