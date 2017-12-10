namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System.Reflection;
    using Soap.If.Interfaces.Messages;

    public class MessageRoute_MessageAssemblyToHttpEndpoint : MessageRoute_Http
    {
        protected MessageRoute_MessageAssemblyToHttpEndpoint(Assembly messagesAssembly, string httpEndpointAddress)
        {
            EndpointAddressHttp = new EndpointAddress_Http(httpEndpointAddress);

            AssemblyName = messagesAssembly.FullName;
        }

        public string AssemblyName { get; set; }

        public EndpointAddress_Http EndpointAddressHttp { get; set; }

        protected override string From => AssemblyName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().Assembly.FullName == AssemblyName;
        }
    }
}