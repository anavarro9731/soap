namespace Sample.Logic.Mappers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Pf.MessageContractsBase.Commands;
    using Soap.Utility.Objects.Blended;

    public class UpgradeDatabaseMapping : IMessageFunctions<UpgradeTheDatabaseCommand>, ICanCall<IOperation>, ICanCall<IProcess>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper() => new Dictionary<ErrorCode, ErrorCode>();

        public Task Handle(UpgradeTheDatabaseCommand msg)
        {
            return this.Get<UpgradeTheDatabaseProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries<UpgradeTheDatabaseCommand> msg)
        {
            var failed = msg.FailedMessage;

            //* get context and put logic here or in a process? operation doesn't make sense cause its message specific really
            return Task.CompletedTask;
        }

        public void Validate(UpgradeTheDatabaseCommand msg)
        {
            new UpgradeTheDatabaseCommand.Validator().ValidateAndThrow(msg);
        }
    }
}