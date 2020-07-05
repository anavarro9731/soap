namespace Soap.Pf.LogicBase
{
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using Soap.MessagePipeline.Context;

    public static class ClearDatabase
    {
        /// <summary>
        /// *** WARNING DANGER USE ONLY UNDER SUPERVISION ***
        /// </summary>
        /// <returns></returns>
        public static async Task ExecuteOutsideTransactionUsingCurrentContext()
        {
            var context = ContextWithMessageLogEntry.Current;
            var repo = context.DataStore.DocumentRepository;

            //* delete everything
            await ((IResetData)repo).NonTransactionalReset();

            //* re-add the entry for the current message 
            var newSession = new DataStore(repo);

            await newSession.Create(context.MessageLogEntry);
            await newSession.CommitChanges();
        }
    }
}