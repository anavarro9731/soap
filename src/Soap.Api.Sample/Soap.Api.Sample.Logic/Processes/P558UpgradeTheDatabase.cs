﻿namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces;
    using Sample.Constants;
    using Sample.Logic.Operations;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public class P558UpgradeTheDatabase : Process, IBeginProcess<C101UpgradeTheDatabase>
    {
        public Func<C101UpgradeTheDatabase, Task> BeginProcess =>
            async message =>
                {
                if (message.ReSeed)
                {
                    await ExecuteOutsideTransactionUsingCurrentContext();
                }

                switch (message.ReleaseVersion)
                {
                    case ReleaseVersions.V1:
                        await V1();
                        break;
                    case ReleaseVersions.V2:
                        await V2();
                        break;
                    default:
                        Guard.Against(true, ErrorCodes.NoUpgradeScriptExistsForThisVersion);
                        break;
                }
                };

        /// <summary>
        ///     *** WARNING DANGER USE ONLY UNDER SUPERVISION THIS IS REALLY A HIDDEN CAPABILITY OF DATASTORE
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