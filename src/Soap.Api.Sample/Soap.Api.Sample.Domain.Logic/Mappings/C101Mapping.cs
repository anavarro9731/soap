namespace Sample.Logic.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Sample.Logic.Operations;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C101Mapping : IMessageFunctionsClientSide<C101UpgradeTheDatabase>
    {
        /* Error Codes only need to be mapped if there is front-end logic that might depend on them
         otherwise the default error handling logic will do the job of returning the error message but without a specific code. */
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper =>
            new Dictionary<ErrorCode, ErrorCode>
            {
                {
                    C101UpgradeTheDatabase.ErrorCodes.NoUpgradeScriptExistsForThisVersion,
                    UpgradeTheDatabaseProcess.ErrorCodes.NoUpgradeScriptExistsForThisVersion
                },
                {
                    C101UpgradeTheDatabase.ErrorCodes.AttemptingToUpgradeDatabaseToOutdatedVersion,
                    ServiceStateOperations.ErrorCodes.AttemptingToUpgradeDatabaseToOutdatedVersion
                }
            };

        public Type[] MessageCanContinueTheseStatefulProcesses { get; }

        public Task Handle(C101UpgradeTheDatabase msg)
        {
            return this.Get<UpgradeTheDatabaseProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries<C101UpgradeTheDatabase> msg)
        {
            return this.Get<NotifyOfFinalFailureProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public void Validate(C101UpgradeTheDatabase msg)
        {
            new C101UpgradeTheDatabase.Validator().ValidateAndThrow(msg);
        }
    }
}