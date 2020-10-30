namespace Soap.Api.Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;

    public class C105SendLargeMessage : ApiCommand
    {
        public override ApiPermission Permission { get; }
    }
}