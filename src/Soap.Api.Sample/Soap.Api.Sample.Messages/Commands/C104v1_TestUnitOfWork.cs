//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;


    
    public class C104v1_TestUnitOfWork : ApiCommand
    {
        public string C104_HansSoloNewName { get; set; }

        public override void Validate()
        {
        }

        public class C104Validator : AbstractValidator<C104v1_TestUnitOfWork>
        {
        }
    }
}
