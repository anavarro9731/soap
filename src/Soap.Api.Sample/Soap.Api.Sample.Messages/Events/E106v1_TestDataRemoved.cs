namespace Soap.Api.Sample.Messages.Events
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class E106v1_TestDataRemoved : ApiEvent
    {
        public E106v1_TestDataRemoved(Guid testDataId)
        {
            E106_TestDataId = testDataId;
        }

        public E106v1_TestDataRemoved()
        {
        }

        public Guid? E106_TestDataId { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<E106v1_TestDataRemoved>
        {
            public Validator()
            {
                RuleFor(x => x.E106_TestDataId).NotEmpty();
            }
        }
    }
}