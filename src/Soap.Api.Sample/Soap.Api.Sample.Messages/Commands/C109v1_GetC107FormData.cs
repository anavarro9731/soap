namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    /// <summary>
    /// This command get's the structure and data for the AutoForm UI control
    /// It is marked as NoAuth, because sometimes form may not require it (e.g. newsletter signup)
    /// But it will check in the pipeline that the form you are requesting you have access
    /// to the command it sends.
    /// </summary>
    [AuthorisationNotRequired]
    public class C109v1_GetC107FormData : ApiCommand
    {
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C109v1_GetC107FormData>
        {
        }
    }
}
