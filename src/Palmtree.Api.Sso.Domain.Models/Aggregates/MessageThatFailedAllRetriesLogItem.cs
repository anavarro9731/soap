namespace Palmtree.Api.Sso.Domain.Models.Aggregates
{
    using System;
    using DataStore.Providers.CosmosDb;

    public class MessageFailedAllRetriesLogItem : CosmosAggregate
    {
        public Guid IdOfMessageThatFailed { get; set; }
    }
}