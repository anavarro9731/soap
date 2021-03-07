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
            C101_ReleaseVersion = new TypedEnumerationFlags<ReleaseVersions>(releaseVersion, false);
        }

        public C101v1_UpgradeTheDatabase()
        {
            //serialiser
        }

        public TypedEnumerationFlags<ReleaseVersions> C101_ReleaseVersion { get; set; } =
            new TypedEnumerationFlags<ReleaseVersions>(null, false);

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C101v1_UpgradeTheDatabase>
        {
            public Validator()
            {
                RuleFor(x => x.C101_ReleaseVersion).NotEmpty();
                RuleFor(x => x.C101_ReleaseVersion.SelectedKeys).NotEmpty().WithMessage("Must select a version");
            }
        }
    }
}
