//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class E104v1_TestDataUpserted : ApiEvent
    {
        public E104v1_TestDataUpserted(Guid testDataId)
        {
            E104_TestDataId = testDataId;
        }

        public E104v1_TestDataUpserted()
        {
            
        }

        public Guid? E104_TestDataId { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<E104v1_TestDataUpserted>
        {
            public Validator()
            {
                RuleFor(x => x.E104_TestDataId).NotEmpty();
            }
        }
    }
}
