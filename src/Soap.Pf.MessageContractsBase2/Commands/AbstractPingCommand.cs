namespace Soap.Pf.MessageContractsBase.CommSands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public abstract class AbstractPingCommand<TPongResponse> : ApiCommand<TPongResponse> where TPongResponse : ApiEvent, new()
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

        public override sealed void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public abstract class AbstractPongEvent : ApiEvent
        {
            public DateTime PingedAt { get; set; }

            public string PingedBy { get; set; }

            public DateTime PongedAt { get; set; }

            public string PongedBy { get; set; }

            public override void Validate()
            {
                new Validator().ValidateAndThrow(this);
            }

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

        public class Validator : AbstractValidator<AbstractPingCommand<TPongResponse>> 
        {
            public Validator()
            {
                RuleFor(x => x.PingedBy).NotEmpty();
                RuleFor(x => x.PingedAt).NotEmpty();
            }
        }
    }
}