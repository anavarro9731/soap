namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C109v1_GetC107FormDataForCreate : ApiCommand
    {
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C109v1_GetC107FormDataForCreate>
        {
        }
    }
}
