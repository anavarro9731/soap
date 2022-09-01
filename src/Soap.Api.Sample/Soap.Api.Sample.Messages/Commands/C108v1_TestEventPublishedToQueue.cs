//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    [AuthenticationNotRequired]
    [AuthorisationNotRequired]
    public class C108v1_TestEventPublishedToQueue : ApiCommand
    {
        public bool? C108_SetQueueHeaderOnly { get; set; }
        
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C108v1_TestEventPublishedToQueue>
        {
        }
    }
}