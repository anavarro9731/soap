//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;
    
    public class C104TestUnitOfWork : ApiCommand
    {
        public string HansSoloNewName { get; set; }

        public override ApiPermission Permission { get; }

        public class C104Validator : AbstractValidator<C104TestUnitOfWork>
        {
        }
    }
}