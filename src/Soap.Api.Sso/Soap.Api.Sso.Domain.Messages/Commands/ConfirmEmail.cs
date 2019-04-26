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
    }

    public class ConfirmEmailValidator : AbstractValidator<ConfirmEmail>
    {
        public ConfirmEmailValidator()
        {
            RuleFor(x => x.StatefulProcessId).NotNull();
        }
    }
}