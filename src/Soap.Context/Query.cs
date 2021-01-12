namespace Soap.Context
{
    using DataStore;
    using DataStore.Interfaces;
    using Soap.Context.Context;
    using Soap.Interfaces;

    public class Query : IQuery
    {
        protected static DataStoreReadOnly DataReader => ContextWithMessageLogEntry.Current.DataStore.AsReadOnly();
        protected static IWithoutEventReplay DataReaderWithoutEventReplay => ContextWithMessageLogEntry.Current.DataStore.WithoutEventReplay;
    }
}
