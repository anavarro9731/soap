namespace Soap.Api.Sample.Domain.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using FluentValidation;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.Api.Sample.Domain.Logic.Operations;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.If.Utility.PureFunctions;

    public class UpgradeTheDatabaseProcess : Process<UpgradeTheDatabaseProcess>, IBeginProcess<UpgradeTheDatabaseCommand>
    {
        private readonly IDocumentRepository documentRepository;

        private readonly ServiceStateOperations serviceStateOperations;

        public UpgradeTheDatabaseProcess(ServiceStateOperations serviceStateOperations, IDocumentRepository documentRepository)
        {
            this.serviceStateOperations = serviceStateOperations;
            this.documentRepository = documentRepository;
        }

        public async Task BeginProcess(UpgradeTheDatabaseCommand message, ApiMessageMeta meta)
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

            //TODO: move to framework
            async Task ClearDb()
            {
                MessageLogItem envelopeMessage = null;

                if (message.EnvelopeId.HasValue)
                {
                    envelopeMessage = await DataStoreReadOnly.ReadActiveById<MessageLogItem>(message.EnvelopeId.Value);
                }

                await ((IResetData)this.documentRepository).NonTransactionalReset();

                if (envelopeMessage != null)
                {
                    await this.documentRepository.AddAsync(
                        new ReplaceMessageLogItemOperation
                        {
                            Model = envelopeMessage
                        });
                }

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
                await this.serviceStateOperations.SetDatabaseVersion(ReleaseVersions.v2);
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

        public class ReplaceMessageLogItemOperation : IDataStoreWriteOperation<MessageLogItem>
        {
            public DateTime Created { get; set; }

            public string MethodCalled { get; set; }

            public MessageLogItem Model { get; set; }

            public double StateOperationCost { get; set; }

            public TimeSpan? StateOperationDuration { get; set; }

            public long StateOperationStartTimestamp { get; set; }

            public long? StateOperationStopTimestamp { get; set; }

            public string TypeName { get; set; }
        }
    }
}