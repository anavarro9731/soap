namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

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