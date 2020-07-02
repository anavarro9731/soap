namespace Soap.Pf.MessageContractsBase.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces;

    public abstract class AbstractPingCommand<TPongResponse> : ApiCommand<TPongResponse> where TPongResponse : ApiEvent, new()
    {
        public DateTime PingedAt { get; set; }

        public string PingedBy { get; set; }

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