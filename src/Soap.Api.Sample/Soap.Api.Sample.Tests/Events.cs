namespace Soap.Api.Sample.Tests
{
    using System;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share message instances and have concurrency problems otherwise */

    public static partial class Events
    {
        public static E100v1_Pong E100v1_Pong =>
            new E100v1_Pong
            {
                E000_PongedAt = DateTime.UtcNow
            };
    }
}
