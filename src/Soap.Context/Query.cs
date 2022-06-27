namespace Soap.Context
{
    using DataStore;
    using DataStore.Interfaces;
    using Soap.Context.Context;
    using Soap.Interfaces;

    public class Query : IQuery
    {
        protected static IDataStoreReadOnly DataReader => ContextWithMessageLogEntry.Current.DataStore.AsReadOnly();
    }
}
