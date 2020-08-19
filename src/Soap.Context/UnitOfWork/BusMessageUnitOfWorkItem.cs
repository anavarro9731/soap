﻿namespace Soap.MessagePipeline.UnitOfWork
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Newtonsoft.Json;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline.Logging;
    using Soap.Utility.Models;

    public class BusMessageUnitOfWorkItem : SerialisableObject
    {
        public BusMessageUnitOfWorkItem(ApiMessage x)
            : base(x)
        {
            MessageId = x.Headers.GetMessageId();
        }

        public BusMessageUnitOfWorkItem()
        {
            //- serialiser
        }

        [JsonProperty]
        public Guid MessageId { get; internal set; }
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
            var logEntry = (await dataStore.Read<MessageLogEntry>(x => x.id == item.MessageId)).SingleOrDefault();
            return logEntry != null;
        }
    }
}