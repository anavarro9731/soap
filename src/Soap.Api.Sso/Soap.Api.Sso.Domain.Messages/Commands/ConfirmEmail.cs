namespace Soap.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.If.Interfaces.Messages;

    public class ConfirmEmail : ApiCommand
    {
        public ConfirmEmail(Guid processId)
        {
            StatefulProcessId = processId;
        }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<ConfirmEmail>
        {
            public Validator()
            {
                RuleFor(x => x.StatefulProcessId).NotNull();
            }
        }
    }
}