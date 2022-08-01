//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C114v1_DeleteTestDataById : ApiCommand
    {
        public Guid? C114_TestDataId { get; set; }
        
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C114v1_DeleteTestDataById>
        {
            public Validator()
            {
                RuleFor(x => x.C114_TestDataId).NotEmpty();
            }
        }
    }
}
