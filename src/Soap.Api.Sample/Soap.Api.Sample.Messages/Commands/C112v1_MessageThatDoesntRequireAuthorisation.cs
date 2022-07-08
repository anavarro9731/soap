//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Messages.Commands
{
    using CircuitBoard;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    [AuthorisationNotRequired]
    [AuthenticationNotRequired]
    public sealed class C112v1_MessageThatDoesntRequireAuthorisation : ApiCommand
    {
        public TypedEnumerationAndFlags<ForwardAction> C112_NextAction { get; set; }

        public ForwardAction C112_Test { get; set; }
        
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class ForwardAction : TypedEnumeration<ForwardAction>
        {
            public static ForwardAction DoNothing = Create(nameof(DoNothing), nameof(DoNothing));

            public static ForwardAction SendAnotherCommandThatDoesntRequireAuthorisation = Create(
                nameof(SendAnotherCommandThatDoesntRequireAuthorisation),
                nameof(SendAnotherCommandThatDoesntRequireAuthorisation));

            public static ForwardAction SendAnotherCommandThatDoesRequireAuthorisation = Create(
                nameof(SendAnotherCommandThatDoesRequireAuthorisation),
                nameof(SendAnotherCommandThatDoesRequireAuthorisation));
        }

        public class Validator : AbstractValidator<C112v1_MessageThatDoesntRequireAuthorisation>
        {
        }
    }
}
