namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class SeedDatabase : ApiCommand
    {
    }

    public class SeedDatabaseValidator : AbstractValidator<SeedDatabase>
    {
    }
}