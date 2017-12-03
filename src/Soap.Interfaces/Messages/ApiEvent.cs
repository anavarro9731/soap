﻿namespace Soap.If.Interfaces.Messages
{
    using System;

    public abstract class ApiEvent : ApiMessage, IApiEvent
    {
        public DateTime OccurredAt { get; set; }
    }
}