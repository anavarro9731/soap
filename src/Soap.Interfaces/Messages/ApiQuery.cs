﻿namespace Soap.If.Interfaces.Messages
{
    /* these two classes should not inherit from each other so as to allow them to be constrained separately in handlers
     */

    public class ApiQuery : ApiMessage, IApiQuery
    {
    }

    public class ApiQuery<TReturnValue> : ApiMessage, IApiQuery where TReturnValue : class, new()
    {
        public TReturnValue ReturnValue { get; set; }
    }
}