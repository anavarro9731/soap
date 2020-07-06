namespace Soap.Pf.MessageContractsBase
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public abstract class AbstractPongEvent : ApiEvent
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public DateTime PongedAt { get; set; }

        public string PongedBy { get; set; }

        public class Validator : AbstractValidator<AbstractPongEvent>
        {
            public Validator()
            {
                RuleFor(x => x.PingedBy).NotEmpty();
                RuleFor(x => x.PingedAt).NotEmpty();
                RuleFor(x => x.PongedAt).NotEmpty();
                RuleFor(x => x.PongedBy).NotEmpty();
            }
        }
    }
}