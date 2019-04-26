namespace Soap.Api.Sso.Endpoint.Msmq.Handlers.Commands
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Api.Sso.Domain.Logic.Processes;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.ProcessesAndOperations;
    using Soap.Pf.MsmqEndpointBase;

    public class UpgradeTheDatabaseHandler : CommandHandler<UpgradeTheDatabase>, IMapErrorCodesFromDomainToMessageErrorCodes
    {
        private readonly IProcess<UpgradeTheDatabaseProcess> process;

        public UpgradeTheDatabaseHandler(IProcess<UpgradeTheDatabaseProcess> process)
        {
            this.process = process;
        }

        public Dictionary<ErrorCode, ErrorCode> DefineMapper()
        {
            return new Dictionary<ErrorCode, ErrorCode>
            {
                {
                    UpgradeTheDatabaseProcess.ErrorCodes.AttemptingToUpgradeDatabaseToOutdatedVersion, UpgradeTheDatabase.ErrorCodes.AttemptingToUpgradeDatabaseToOutdatedVersion
                },
                {
                    UpgradeTheDatabaseProcess.ErrorCodes.NoUpgradeScriptExistsForThisVersion, UpgradeTheDatabase.ErrorCodes.NoUpgradeScriptExistsForThisVersion
                }
            };
        }

        protected override async Task Handle(UpgradeTheDatabase message, ApiMessageMeta meta)
        {
            await this.process.BeginProcess(message, meta);
        }
    }
}