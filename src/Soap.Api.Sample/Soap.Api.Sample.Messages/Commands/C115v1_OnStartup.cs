
    namespace Soap.Api.Sample.Messages.Commands
    {
        using Soap.PfBase.Messages;
        using Soap.Interfaces.Messages;
        using FluentValidation;


        public class C115v1_OnStartup : ApiCommand
        {
            public override void Validate()
            {
                new Validator().ValidateAndThrow(this);
            }
    
            public class Validator : AbstractValidator<C115v1_OnStartup>
            {
                public Validator()
                {
                }
            }
        }
    }
