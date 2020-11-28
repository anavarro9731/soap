namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Microsoft.VisualBasic;
    using Soap.Interfaces.Messages;

    public class C106v1_LargeCommand : ApiCommand
    {
        public class C106Validator : AbstractValidator<C106v1_LargeCommand>
        {
            public C106Validator()
            {
                RuleFor(x => x.C106_Large256KbString).NotEmpty();
            }
        }
        
        public string? C106_Large256KbString { get; set; } = new String('*',256000);

    }
}
