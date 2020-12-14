namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class C102v1_GetServiceState : ApiCommand
    {

        public class Validator : AbstractValidator<C102v1_GetServiceState>
        {
        }

        public override void Validate()
        {
            
        }
    }
}
