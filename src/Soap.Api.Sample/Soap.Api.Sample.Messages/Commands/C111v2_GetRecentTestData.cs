namespace Soap.Api.Sample.Messages.Commands
{
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public sealed class C111v2_GetRecentTestData : ApiCommand
    {
        public int? MaxAgeInDays { get; set; }
        public int? MaxRecords { get; set; }
        
        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C111v2_GetRecentTestData>
        {
        }
    }
}
