namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Operations;

    public class P204UpgradeTheDatabase : Process, IBeginProcess<C101v1_UpgradeTheDatabase>
    {
        public Func<C101v1_UpgradeTheDatabase, Task> BeginProcess =>
            async message =>
                {
                if (message.C101_ReSeed.GetValueOrDefault())
                {
                    await ExecuteOutsideTransactionUsingCurrentContext();
                }

                switch (message.C101_ReleaseVersion)
                {
                    case var v when v.HasFlag(ReleaseVersions.V1):
                        await V1();
                        break;
                    case var v when v.HasFlag(ReleaseVersions.V2):
                        await V2();
                        break;
                    default:
                        Guard.Against(true, ErrorCodes.NoUpgradeScriptExistsForThisVersion);
                        break;
                }
                };

        /// <summary>
        ///     *** WARNING: THIS IS REALLY A HIDDEN CAPABILITY OF DATASTORE
        ///     BEING USED ONLY FOR THE VERY UNIQUE CASE OF RESEEDING THE DATABASE AND WOULD NOT BE PART OF ANY
        ///     NORMAL BUSINESS LOGIC ***
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

        private async Task V1()
        {
            await SetInitialServiceState();

            Task SetInitialServiceState()
            {
                return this.Get<ServiceStateOperations>().Call(x => x.CreateServiceState)();
            }
        }

        private async Task V2()
        {
            await SetDbVersion();

            Task SetDbVersion()
            {
                return this.Get<ServiceStateOperations>().Call(x => x.SetDatabaseVersion)(ReleaseVersions.V2);
            }
        }

        public class ErrorCodes : ErrorCode
        {
            public static readonly ErrorCode NoUpgradeScriptExistsForThisVersion = Create(
                Guid.Parse("8b19f630-0ccf-4b2d-91bb-4deb72ce3676"),
                "No Upgrade Script Exists For This Version");
        }
    }
}
