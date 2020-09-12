namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C103StartPingPong : ApiCommand
    {
        public override ApiPermission Permission { get; }

        public class C103Validator : AbstractValidator<C103StartPingPong>
        {
        }
    }
}