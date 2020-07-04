﻿namespace Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Sample.Models.Constants;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class UpgradeTheDatabaseCommand : ApiCommand
    {
        public UpgradeTheDatabaseCommand(ReleaseVersions releaseVersion)
        {
            ReleaseVersion = releaseVersion;
        }

        public UpgradeTheDatabaseCommand()
        {
        }

        public Guid? EnvelopeId { get; set; }

        public ReleaseVersions ReleaseVersion { get; set; }

        public bool ReSeed { get; set; }

        public class ErrorCodes : ErrorCode
        {
            public static readonly ErrorCode AttemptingToUpgradeDatabaseToOutdatedVersion = Create(
                Guid.Parse("dd4f97b0-e659-4709-a7d2-881c59974fba"),
                "Attempting To Upgrade Database To Outdated Version");

            public static readonly ErrorCode NoUpgradeScriptExistsForThisVersion = Create(
                Guid.Parse("493ab843-c82a-4623-9ede-aa7dabba86f4"),
                "No Upgrade Script Exists For This Version");
        }

        public class Validator : AbstractValidator<UpgradeTheDatabaseCommand>
        {
            public Validator()
            {
                RuleFor(x => x.ReleaseVersion != ReleaseVersions.Null);
            }
        }
    }
}