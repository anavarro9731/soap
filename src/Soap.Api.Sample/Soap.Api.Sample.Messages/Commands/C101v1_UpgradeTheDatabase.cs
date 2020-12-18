namespace Soap.Api.Sample.Messages.Commands
{
    using CircuitBoard;
    using FluentValidation;
    using Soap.Api.Sample.Constants;
    using Soap.Interfaces.Messages;

    public class C101v1_UpgradeTheDatabase : ApiCommand
    {
        public C101v1_UpgradeTheDatabase(ReleaseVersions releaseVersion)
        {
            C101_ReleaseVersion = new TypedEnumerationAndFlags<ReleaseVersions>(releaseVersion);
        }

        public C101v1_UpgradeTheDatabase()
        {
        }

        public TypedEnumerationAndFlags<ReleaseVersions> C101_ReleaseVersion { get; set; }

        public bool? C101_ReSeed { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C101v1_UpgradeTheDatabase>
        {
            public Validator()
            {
                RuleFor(x => x.C101_ReleaseVersion).NotNull();
            }
        }
    }
}
