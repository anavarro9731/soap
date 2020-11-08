﻿namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Api.Sample.Constants;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Blended;

    public class C101UpgradeTheDatabase : ApiCommand
    {
        public C101UpgradeTheDatabase(ReleaseVersions releaseVersion)
        {
            ReleaseVersion = releaseVersion;
        }

        public C101UpgradeTheDatabase()
        {
        }

        public Guid? EnvelopeId { get; set; }

        public override ApiPermission Permission { get; }

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
        
        public class ReleaseVersions : Enumeration<ReleaseVersions>
        {
            public static ReleaseVersions V1 = Create("v1", "Version 1");
            public static ReleaseVersions V2 = Create("v2", "Version 2");
        }

        public class Validator : AbstractValidator<C101UpgradeTheDatabase>
        {
            public Validator()
            {
                RuleFor(x => x.ReleaseVersion).NotNull();
            }
        }
    }
}