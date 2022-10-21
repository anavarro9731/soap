namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Soap.Interfaces.Messages;

    public class C116v1_TruncateMessageLog : ApiCommand
    {
        public DateTime? C116_From { get; set; }

        public DateTime? C116_To { get; set; }

        public override void Validate()
        {
            new Validator().ValidateAndThrow(this);
        }

        public class Validator : AbstractValidator<C116v1_TruncateMessageLog>
        {
        }
    }
}