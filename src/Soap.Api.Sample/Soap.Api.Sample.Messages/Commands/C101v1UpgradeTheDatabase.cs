namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using CircuitBoard;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C101v1UpgradeTheDatabase : ApiCommand
    {
        public C101v1UpgradeTheDatabase(ReleaseVersions releaseVersion)
        {
            C101_ReleaseVersion = releaseVersion;
        }

        public C101v1UpgradeTheDatabase()
        {
        }

        public Guid? C101_EnvelopeId { get; set; }

        public ReleaseVersions C101_ReleaseVersion { get; set; }

        public bool C101_ReSeed { get; set; }


        public class ReleaseVersions : Enumeration<ReleaseVersions>
        {
            public static ReleaseVersions V1 = Create("v1", "Version 1");

            public static ReleaseVersions V2 = Create("v2", "Version 2");
        }

        public class Validator : AbstractValidator<C101v1UpgradeTheDatabase>
        {
            public Validator()
            {
                RuleFor(x => x.C101_ReleaseVersion).NotNull();
            }
        }
    }
}
