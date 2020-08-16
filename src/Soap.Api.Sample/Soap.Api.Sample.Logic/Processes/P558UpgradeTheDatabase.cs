namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Constants;
    using Sample.Logic.Operations;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.PfBase.Logic;
    using Soap.Utility.Functions.Operations;
    using Soap.Utility.Objects.Blended;

    public class P558UpgradeTheDatabase : Process, IBeginProcess<C101UpgradeTheDatabase>
    {
        public Func<C101UpgradeTheDatabase, Task> BeginProcess =>
            async message =>
                {
                if (message.ReSeed)
                {
                    await ClearDatabase.ExecuteOutsideTransactionUsingCurrentContext();
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