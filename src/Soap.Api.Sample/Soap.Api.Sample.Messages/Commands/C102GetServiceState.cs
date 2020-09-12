namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class C102GetServiceState : ApiCommand
    {
        public override ApiPermission Permission { get; }

        public class Validator : AbstractValidator<C102GetServiceState>
        {
        }
    }
}