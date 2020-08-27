namespace Soap.MessagePipeline
{
    using DataStore;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Context;

    public class Query : IQuery
    {
        protected static DataStoreReadOnly DataReader => ContextWithMessageLogEntry.Current.DataStore.AsReadOnly();
    }
}