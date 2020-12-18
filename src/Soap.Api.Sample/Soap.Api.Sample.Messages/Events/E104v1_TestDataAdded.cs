namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class E104v1_TestDataAdded : ApiEvent
    {
        public E104v1_TestDataAdded(Guid testDataId)
        {
            E104_TestDataId = testDataId;
        }

        public E104v1_TestDataAdded()
        {
            
        }

        public Guid? E104_TestDataId { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<E104v1_TestDataAdded>
        {
            public Validator()
            {
                RuleFor(x => x.E104_TestDataId).NotEmpty();
            }
        }
    }
}
