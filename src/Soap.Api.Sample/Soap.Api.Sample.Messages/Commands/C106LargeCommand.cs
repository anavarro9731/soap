namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Microsoft.VisualBasic;
    using Soap.Interfaces.Messages;

    public class C106LargeCommand : ApiCommand
    {
        public class C106Validator : AbstractValidator<C106LargeCommand>
        {
            public C106Validator()
            {
                RuleFor(x => x.Large256KbString).NotEmpty();
            }
        }
        
        // ReSharper disable once StringLiteralTypo
        public string Large256KbString { get; set; } = new String('*',256000);

        public override ApiPermission Permission { get; }
    }
}