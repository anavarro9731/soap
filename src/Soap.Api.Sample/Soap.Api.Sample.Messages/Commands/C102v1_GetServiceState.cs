//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class C102v1_GetServiceState : ApiCommand
    {
        public override void Validate()
        {
        }

        public class Validator : AbstractValidator<C102v1_GetServiceState>
        {
        }
    }
}
