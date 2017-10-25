namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class SeedDatabase : ApiCommand
    {
    }

    public class SeedDatabaseValidator : AbstractValidator<SeedDatabase>
    {
    }
}