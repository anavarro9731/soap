﻿namespace Sample.Messages.Commands
{
    using System;
    using System.Data;
    using System.Linq;
    using FluentValidation;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Blended;

    public class C104TestUnitOfWork : ApiCommand
    {
        public override ApiPermission Permission { get; }

        public string HansSoloNewName { get; set; }
        
        public class C104Validator : AbstractValidator<C104TestUnitOfWork>
        {
            public C104Validator()
            {
                
            }
        }
        
    }
}