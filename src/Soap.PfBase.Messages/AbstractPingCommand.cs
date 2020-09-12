namespace Soap.Pf.MessageContractsBase
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public abstract class AbstractPingCommand : ApiCommand
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public class Validator : AbstractValidator<AbstractPingCommand>
        {
            public Validator()
            {
                RuleFor(x => x.PingedBy).NotEmpty();
                RuleFor(x => x.PingedAt).NotEmpty();
            }
        }
    }
}