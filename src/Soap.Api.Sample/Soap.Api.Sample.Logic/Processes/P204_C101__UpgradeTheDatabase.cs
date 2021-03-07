namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore;
    using DataStore.Interfaces;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Logic.Queries;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility;

    public class P204_C101__UpgradeTheDatabase : Process, IBeginProcess<C101v1_UpgradeTheDatabase>
    {
        public Func<C101v1_UpgradeTheDatabase, Task> BeginProcess =>
            async message =>
                {
                {
                    var serviceState = await new ServiceStateQueries().GetServiceStateById() ?? await SetInitialServiceState();

                    var currentVersionAsInt = serviceState.DatabaseState.SelectedKeys.Any()
                                                  ? serviceState.DatabaseState.SelectedKeys.Select(int.Parse).Max()
                                                  : 0; //* it's new

                    var requestedVersionKey =
                        message.C101_ReleaseVersion.SelectedKeys.Single(); //* message validator should ensure this succeeds
                    var requestedVersionAsInt = int.Parse(requestedVersionKey);

                    //* messageVersion is equal
                    if (currentVersionAsInt == requestedVersionAsInt) return;

                    //* messageVersion is less
                    Guard.Against(
                        currentVersionAsInt > requestedVersionAsInt,
                        $"You are asking to upgrade the database to version {requestedVersionAsInt} which is lower than the current version {currentVersionAsInt}.");

                    //* messageVersion is greater than currentVersion

                    var allVersionsWhoseScriptsNeedRunning = new TypedEnumerationFlags<ReleaseVersions>();

                    foreach (var version in ReleaseVersions.GetAllInstances())
                    {
                        var versionAsInt = int.Parse(version.Key);
                        if (versionAsInt > currentVersionAsInt && versionAsInt <= requestedVersionAsInt)
                        {
                            allVersionsWhoseScriptsNeedRunning.AddFlag(version);
                        }
                    }

                    if (allVersionsWhoseScriptsNeedRunning.HasFlag(ReleaseVersions.V1))
                    {
                        //* do other stuff
                        SetDbVersion(ReleaseVersions.V1);
                    }

                    if (allVersionsWhoseScriptsNeedRunning.HasFlag(ReleaseVersions.V2))
                    {
                        //* do other stuff
                        SetDbVersion(ReleaseVersions.V2);
                    }
                }

                Task<ServiceState> SetInitialServiceState()
                {
                    return this.Get<ServiceStateOperations>().Call(x => x.CreateServiceState)();
                }

                Task SetDbVersion(ReleaseVersions newVersion)
                {
                    return this.Get<ServiceStateOperations>().Call(x => x.SetDatabaseVersion)(newVersion);
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

        public class ErrorCodes : ErrorCode
        {
            public static readonly ErrorCode NoUpgradeScriptExistsForThisVersion = Create(
                Guid.Parse("8b19f630-0ccf-4b2d-91bb-4deb72ce3676"),
                "No Upgrade Script Exists For This Version");
        }
    }
}
