namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using FluentValidation;
    using Microsoft.VisualBasic;
    using Soap.Interfaces.Messages;

    public class C106v1LargeCommand : ApiCommand
    {
        public class C106Validator : AbstractValidator<C106v1LargeCommand>
        {
            public C106Validator()
            {
                RuleFor(x => x.C106_Large256KbString).NotEmpty();
            }
        }
        
        // ReSharper disable once StringLiteralTypo
        public string C106_Large256KbString { get; set; } = new String('*',256000);

        public override ApiPermission Permission { get; }
    }
}