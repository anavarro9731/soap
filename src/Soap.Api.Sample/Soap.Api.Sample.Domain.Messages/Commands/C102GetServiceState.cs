namespace Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces;

    public sealed class C102GetServiceState : ApiCommand
        {
        public class Validator : AbstractValidator<C102GetServiceState>
        {
        }
    }
}