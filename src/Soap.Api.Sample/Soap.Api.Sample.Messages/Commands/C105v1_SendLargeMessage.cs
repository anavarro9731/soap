namespace Soap.Api.Sample.Messages.Commands
{
    using System;
    using Soap.Interfaces.Messages;

    public class C105v1_SendLargeMessage : ApiCommand
    {
        public Guid? C105_C106Id { get; set; }

        public override void Validate()
        {
        }
    }
}
