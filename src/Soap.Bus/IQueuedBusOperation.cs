namespace Soap.Bus
{
    using CircuitBoard.Messages;
    using Soap.Interfaces;

    /* this class inherits from IQueuedStateChange following the pattern of Circuitboard and Datastore
     It may seem unecessary and even useless to have a commitclosure (see IQueuedStateChange) and not just store the 
     the queued message. This was originally because all we gated the bus and datastore functions for testing, 
     then it was b/c all of these changes were funneled via IMessageAggregator and then
     the unit of work blindly calls the commitclosure in the unit of work for all state changes, but now
     it only does that for things other datastore and the bus which use fakes, so the only reason now is that
     when saving the unit of work we need a common base class with some properties like committed on it that
     we can use to pull out all changes. In theory is we were willing to refactor datastore (and the bus) 
     to use a revised IQueuedStateChange or another common interface with just Committed property we could
     probably lose IQueuedStateChange */
    public interface IQueuedBusOperation : IQueuedStateChange 
    {
    }
}