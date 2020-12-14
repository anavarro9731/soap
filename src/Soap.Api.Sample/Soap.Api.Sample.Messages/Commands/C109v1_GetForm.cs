namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C109v1_GetForm : ApiCommand
    {
        public string C109_FormDataEventName { get; set; }

        public class Validator : AbstractValidator<C109v1_GetForm>
        {
            public Validator()
            {
                RuleFor(x => x.C109_FormDataEventName).NotEmpty();
            }
        }
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }
    }
}
