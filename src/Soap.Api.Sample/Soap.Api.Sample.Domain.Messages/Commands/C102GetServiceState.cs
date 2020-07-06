namespace Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public sealed class C102GetServiceState : ApiCommand
        {
        public class Validator : AbstractValidator<C102GetServiceState>
        {
        }

        public override ApiPermission Permission { get; }
        }
}