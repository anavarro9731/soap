namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;


    public sealed class C110v1_GetTestDataById : ApiCommand
    {
        public Guid? C110_TestDataId { get; set; }

        public override void Validate()
        { 
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C110v1_GetTestDataById>
        {
            public Validator()
            {
                RuleFor(x => x.C110_TestDataId).NotEmpty();
            }
        }
    }
}
