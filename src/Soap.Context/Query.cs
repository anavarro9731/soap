namespace Soap.Context
{
    using DataStore;
    using Soap.Context.Context;
    using Soap.Interfaces;

    public class Query : IQuery
    {
        protected static DataStoreReadOnly DataReader => ContextWithMessageLogEntry.Current.DataStore.AsReadOnly();
    }
}