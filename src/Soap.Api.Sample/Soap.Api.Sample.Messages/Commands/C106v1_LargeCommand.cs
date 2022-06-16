namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;
    
    public class C106v1_LargeCommand : ApiCommand
    {
        public string C106_Large256KbString { get; set; } = new string('*', 256001); //* force it to upload to blob storage

        public override void Validate()
        {
        }

        public class C106Validator : AbstractValidator<C106v1_LargeCommand>
        {
            public C106Validator()
            {
                RuleFor(x => x.C106_Large256KbString).NotEmpty();
            }
        }
    }
}
