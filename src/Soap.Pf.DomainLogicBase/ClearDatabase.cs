namespace Soap.Pf.LogicBase
{
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using Soap.MessagePipeline.Logging;

    public static class ClearDatabase
    {
        public static async Task ExecuteOutsideTransaction(DataStore dataStore, MessageLogEntry logEntry)
        {
            var repo = dataStore.DocumentRepository;

            //* delete everything
            await ((IResetData)repo).NonTransactionalReset();

            //* re-add the entry for the current message 
            var newSession = new DataStore(repo);

            await newSession.Create(logEntry);
            await newSession.CommitChanges();
        }
    }
}