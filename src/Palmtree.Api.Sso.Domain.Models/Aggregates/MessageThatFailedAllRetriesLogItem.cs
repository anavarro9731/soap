﻿namespace Palmtree.Api.Sso.Domain.Models.Aggregates
{
    using System;
    using DataStore.Interfaces.LowLevel;

    public class MessageFailedAllRetriesLogItem : Aggregate
    {
        public Guid IdOfMessageThatFailed { get; set; }
    }
}