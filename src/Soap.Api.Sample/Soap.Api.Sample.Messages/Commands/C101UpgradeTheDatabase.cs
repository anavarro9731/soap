namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

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