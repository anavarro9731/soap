﻿namespace Palmtree.Sample.Api.Domain.Messages.Commands
{
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

    public class SeedDatabase : ApiCommand
    {
    }

    public class SeedDatabaseValidator : AbstractValidator<SeedDatabase>
    {
    }
}
