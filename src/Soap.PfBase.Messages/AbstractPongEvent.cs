namespace Soap.Pf.MessageContractsBase
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public abstract class AbstractPongEvent : ApiEvent
    {
        public DateTime? C000_PingedAt { get; set; }

        public string? C000_PingedBy { get; set; }

        public Guid? C000_PingReference { get; set; }

        public DateTime? C000_PongedAt { get; set; }

        public string? C000_PongedBy { get; set; }

        public class Validator : AbstractValidator<AbstractPongEvent>
        {
            public Validator()
            {
                RuleFor(x => x.C000_PingedBy).NotEmpty();
                RuleFor(x => x.C000_PingedAt).NotEmpty();
                RuleFor(x => x.C000_PongedAt).NotEmpty();
                RuleFor(x => x.C000_PongedBy).NotEmpty();
            }
        }
    }
}
