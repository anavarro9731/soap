namespace Soap.Api.Sso.Domain.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using FluentValidation;
    using Soap.Api.Sso.Domain.Constants;
    using Soap.Api.Sso.Domain.Logic.Operations;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility.PureFunctions;
    using Soap.Pf.EndpointInfrastructure;

    public class UpgradeTheDatabaseProcess : Process<UpgradeTheDatabaseProcess>, IBeginProcess<UpgradeTheDatabase>
    {
        private readonly IDocumentRepository documentRepository;

        private readonly ServiceStateOperations serviceStateOperations;

        private readonly TagOperations TagOperations;

        private readonly UserOperations userOperations;

        public UpgradeTheDatabaseProcess(
            UserOperations userOperations,
            ServiceStateOperations serviceStateOperations,
            TagOperations TagOperations,
            IDocumentRepository documentRepository)
        {
            this.userOperations = userOperations;
            this.serviceStateOperations = serviceStateOperations;
            this.documentRepository = documentRepository;
            this.TagOperations = TagOperations;
        }

        public async Task BeginProcess(UpgradeTheDatabase message, ApiMessageMeta meta)
        {
            {
                Validate();

                if (message.ReSeed) await ClearDb();

                switch (message.ReleaseVersion)
                {
                    case ReleaseVersions.v1:
                        await V1();
                        break;
                    case ReleaseVersions.v2:
                        await V2();
                        break;
                    default:
                        Guard.Against(true, ErrorCodes.NoUpgradeScriptExistsForThisVersion);
                        break;
                }
            }

            async Task ClearDb()
            {
                
                await ((IResetData)this.documentRepository).NonTransactionalReset();
                await this.documentRepository.AddAsync(new ReplaceMessageLogItemOperation
                {
                    Model = meta.MessageLogItem
                });
            }

            void Validate()
            {
                new UpgradeTheDatabaseValidator().ValidateAndThrow(message);
            }
        }

        public class ReplaceMessageLogItemOperation : IDataStoreWriteOperation<MessageLogItem>
        {
            public DateTime Created { get; set; }

            public string MethodCalled { get; set; }

            public string TypeName { get; set; }

            public double StateOperationCost { get; set; }

            public long StateOperationStartTimestamp { get; set; }

            public long? StateOperationStopTimestamp { get; set; }

            public TimeSpan? StateOperationDuration { get; set; }

            public MessageLogItem Model { get; set; }
        }

        private async Task V1()
        {
            await AddDefaultUser();
            await SetInitialServiceState();

            async Task AddDefaultUser()
            {
                await this.userOperations.AddDefaultUser();
            }

            async Task SetInitialServiceState()
            {
                await this.serviceStateOperations.CreateServiceState();
            }
        }

        private async Task V2()
        {
            await ChangeDefaultUsersEmail();
            await AddTag();
            await SetDbVersion();

            async Task AddTag()
            {
                await this.TagOperations.AddTag(null, "V2 Tag");
            }

            async Task SetDbVersion()
            {
                await this.serviceStateOperations.SetDatabaseVersion(ReleaseVersions.v2);
            }

            async Task ChangeDefaultUsersEmail()
            {
                await this.userOperations.ChangeUserEmail(HardCodedMasterData.RootUser.UserId, "rootuser@v2.com");
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