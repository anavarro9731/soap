namespace Soap.Pf.MessageContractsBase
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public abstract class AbstractPongEvent : ApiEvent
    {
        public DateTime? E000_PingedAt { get; set; }

        public string E000_PingedBy { get; set; }

        public Guid? E000_PingReference { get; set; }

        public DateTime? E000_PongedAt { get; set; }

        public string E000_PongedBy { get; set; }

        public class Validator : AbstractValidator<AbstractPongEvent>
        {
            public Validator()
            {
                RuleFor(x => x.E000_PingedBy).NotEmpty();
                RuleFor(x => x.E000_PingedAt).NotEmpty();
                RuleFor(x => x.E000_PongedAt).NotEmpty();
                RuleFor(x => x.E000_PongedBy).NotEmpty();
            }
        }
    }
}
