namespace Soap.Api.Sample.Messages.Commands
{
    using Soap.Interfaces.Messages;

    public class C105v1SendLargeMessage : ApiCommand
    {
        public override ApiPermission Permission { get; }
    }
}