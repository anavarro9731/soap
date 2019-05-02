﻿namespace Soap.If.Interfaces.Messages
{
    using System;

    /* these two classes should not inherit from each other so as to allow them to be constrained separately in handlers
     but to be grouped together in lists we use the IApiCommand interface
     */

    public abstract class ApiCommand : ApiMessage, IApiCommand
    {
        public Guid? StatefulProcessId { get; set; }
    }

    public abstract class ApiCommand<TResponse> : ApiMessage, IApiCommand where TResponse : class, new()
    {
        public TResponse ReturnValue { get; set; }

        public Guid? StatefulProcessId { get; set; }

        
    }
}