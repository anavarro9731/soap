﻿namespace Soap.Api.Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;

    [NoAuth]
    public class C105v1_SendLargeMessage : ApiCommand
    {
        public override void Validate()
        {
        }
    }
}
