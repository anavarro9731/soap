namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;
    
    public class C106v1_LargeCommand : ApiCommand
    {
        public string C106_Large256KbString { get; set; } = new string('*', 256000); //* force it to upload to blob storage

        //public string C106_Larger2MBString { get; set; } = new string('*', 3000000); //* force it to exceed the 2MB cosmos limit where the unit of work will fail if its not saved in blob storage

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
