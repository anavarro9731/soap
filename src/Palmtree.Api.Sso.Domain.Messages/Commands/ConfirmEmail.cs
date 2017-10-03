namespace Palmtree.Api.Sso.Domain.Messages.Commands
{
    using System;
    using FluentValidation;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;

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
