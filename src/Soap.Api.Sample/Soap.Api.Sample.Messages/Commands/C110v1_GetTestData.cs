namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class C110v1_GetTestData : ApiCommand
    {
        public Guid? C110_TestDataId { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C110v1_GetTestData>
        {
            public Validator()
            {
                RuleFor(x => x.C110_TestDataId).NotEmpty();
            }
        }
    }
}
