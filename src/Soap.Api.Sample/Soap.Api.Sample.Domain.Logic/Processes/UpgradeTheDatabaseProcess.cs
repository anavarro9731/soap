namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Logic.Operations;
    using Sample.Messages.Commands;
    using Sample.Models.Constants;
    using Soap.Interfaces;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.LogicBase;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public class UpgradeTheDatabaseProcess : Process, IBeginProcess<UpgradeTheDatabaseCommand>
    {
        private readonly ServiceStateOperations serviceStateOperations = new ServiceStateOperations();

        public Func<UpgradeTheDatabaseCommand, Task>
            BeginProcess =>
            async (message) =>
                {
                    if (message.ReSeed)
                    {
                        await ClearDatabase.ExecuteOutsideTransaction(this.context.DataStore, this.context.MessageLogEntry);
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

        private async Task V1()
        {
            await SetInitialServiceState();

            async Task SetInitialServiceState()
            {
                await this.serviceStateOperations.CreateServiceState();
            }
        }

        private async Task V2()
        {
            await SetDbVersion();

            async Task SetDbVersion()
            {
                await this.serviceStateOperations.SetDatabaseVersion(ReleaseVersions.V2);
            }
        }

        public class ErrorCodes : ErrorCode
        {
            public static readonly ErrorCodes AttemptingToUpgradeDatabaseToOutdatedVersion = Create<ErrorCodes>(
                Guid.Parse("b866824e-ccc2-4f84-8399-15877bf735e9"),
                "Attempting To Upgrade Database To Outdated Version");

            public static readonly ErrorCodes NoUpgradeScriptExistsForThisVersion = Create<ErrorCodes>(
                Guid.Parse("8b19f630-0ccf-4b2d-91bb-4deb72ce3676"),
                "No Upgrade Script Exists For This Version");
        }

        
    }
}