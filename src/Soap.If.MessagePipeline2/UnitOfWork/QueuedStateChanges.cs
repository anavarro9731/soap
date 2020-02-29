namespace Soap.MessagePipeline.UnitOfWork
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard.Messages;
    using DataStore.Interfaces;
    using DataStore.Models.Messages;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.Logging;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;

    /*
     * Executes queued changes via a closure.
     * With this approach we can support any data access framework easily
     * without requiring polymorphic implementations of an interface(s) for each
     * supported framework.
     *
     * The downside being that these changes can't be rolled back or persisted
     * with the unit of work. 
     */

    public static class ContextAfterMessageLog34534534
    {
    }
}