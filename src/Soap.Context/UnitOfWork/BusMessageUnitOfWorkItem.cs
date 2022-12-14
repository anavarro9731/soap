namespace Soap.Context.UnitOfWork
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs.Models;
    using CircuitBoard;
    using DataStore.Interfaces;
    using Newtonsoft.Json;
    using Soap.Context.Logging;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Models;

    public class BusMessageUnitOfWorkItem : SerialisableObject
    {
        public BusMessageUnitOfWorkItem(ApiMessage x, IBusClient.EventVisibilityFlags eventVisibility, DateTimeOffset? deferUntil)
            : base(x)
        {
            DeferUntil = deferUntil;
            MessageId = x.Headers.GetMessageId();
            EventVisibility = eventVisibility;
        }

        public BusMessageUnitOfWorkItem()
        {
            //- serialiser
        }

        [JsonProperty]
        public Guid MessageId { get; internal set; }
        
        [JsonProperty]
        public DateTimeOffset? DeferUntil { get; internal set; }
        
        [JsonProperty]
        public IBusClient.EventVisibilityFlags EventVisibility { get; internal set; }
    }

    public static class BusMessageUnitOfWorkItemExtensions
    {
        /* used on retries to know the state of a message */
        public static Task<bool> IsComplete(this BusMessageUnitOfWorkItem item, IDataStore dataStore) =>
            QueryForCompleteness(item, dataStore);

        /* checking completeness by using MessageLogEntry record does not guarantee the
         message has not already been sent as race condition can occur,
         however MessageLog constraints will filter any duplicates solving that situation 
         and for our purposes of knowing whether to resend it is sufficiently consistent
         and will avoid duplicates in 99% of cases */
        private static async Task<bool> QueryForCompleteness(this BusMessageUnitOfWorkItem item, IDataStore dataStore)
        {
            var logEntry = (await dataStore.ReadById<MessageLogEntry>(item.MessageId, options => options.ProvidePartitionKeyValues(WeekInterval.FromUtcNow())));
            return logEntry != null;
        }
    }
}
