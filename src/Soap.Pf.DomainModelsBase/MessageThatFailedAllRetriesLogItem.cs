 namespace Soap.Pf.DomainModelsBase
{
    using System;
    using DataStore.Interfaces.LowLevel;

    public class MessageFailedAllRetriesLogItem : Aggregate
    {
        public Guid IdOfMessageThatFailed { get; set; }
    }
}