﻿namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    [Obsolete("Use V2")]
    public sealed class C111v1_GetRecentTestData : ApiCommand
    {
        public int? MaxRecords { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C111v1_GetRecentTestData>
        {
        }
    }
}