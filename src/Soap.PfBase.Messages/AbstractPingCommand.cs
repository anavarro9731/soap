namespace Soap.Pf.MessageContractsBase
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public abstract class AbstractPingCommand : ApiCommand
    {
        public DateTime? C000_PingedAt { get; set; }

        public string C000_PingedBy { get; set; }

        public class Validator : AbstractValidator<AbstractPingCommand>
        {
            public Validator()
            {
                RuleFor(x => x.C000_PingedBy).NotEmpty();
                RuleFor(x => x.C000_PingedAt).NotEmpty();
            }
        }
    }
}
