namespace Soap.Api.Sample.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Api.Sample.Domain.Constants;
    using Soap.If.Interfaces;
    using Soap.If.Interfaces.Messages;

    public class UpgradeTheDatabaseCommand : ApiCommand
    {
        public UpgradeTheDatabaseCommand(ReleaseVersions releaseVersion)
        {
            ReleaseVersion = releaseVersion;
            
        }

        public ReleaseVersions ReleaseVersion { get; set; }

        public bool ReSeed { get; set; }

        public Guid? EnvelopeId { get; set; }

        public class ErrorCodes : ErrorCode
        {
            public static readonly ErrorCodes AttemptingToUpgradeDatabaseToOutdatedVersion = Create<ErrorCodes>(
                Guid.Parse("dd4f97b0-e659-4709-a7d2-881c59974fba"),
                "Attempting To Upgrade Database To Outdated Version");

            public static readonly ErrorCodes NoUpgradeScriptExistsForThisVersion = Create<ErrorCodes>(
                Guid.Parse("493ab843-c82a-4623-9ede-aa7dabba86f4"),
                "No Upgrade Script Exists For This Version");
        }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<UpgradeTheDatabaseCommand>
        {
            public Validator()
            {
                RuleFor(x => x.ReleaseVersion != ReleaseVersions.NULL);
            }
        }
    }


}