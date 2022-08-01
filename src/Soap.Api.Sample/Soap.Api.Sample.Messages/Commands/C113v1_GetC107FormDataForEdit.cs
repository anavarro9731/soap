//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C113v1_GetC107FormDataForEdit : ApiCommand
    {
        public Guid? C113_TestDataId { get; set; }
        
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C113v1_GetC107FormDataForEdit>
        {
            public Validator()
            {
                RuleFor(x => x.C113_TestDataId).NotEmpty();
            }
        }
    }
}
