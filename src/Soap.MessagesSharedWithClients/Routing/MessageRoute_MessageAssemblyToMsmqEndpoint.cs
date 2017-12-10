namespace Soap.Pf.ClientServerMessaging.Routing
{
    using System.Reflection;
    using Soap.If.Interfaces.Messages;

    public class MessageRoute_MessageAssemblyToMsmqEndpoint : MessageRoute_Msmq
    {
        protected MessageRoute_MessageAssemblyToMsmqEndpoint(Assembly messagesAssembly, string msmqEndpointAddress)
        {
            MsmqEndpointAddress = new EndpointAddress_Msmq(msmqEndpointAddress);

            AssemblyName = messagesAssembly.FullName;
        }

        public string AssemblyName { get; set; }

        public EndpointAddress_Msmq MsmqEndpointAddress { get; set; }

        protected override string From => AssemblyName;

        public override bool CanRouteMessage(IApiMessage message)
        {
            return message.GetType().Assembly.FullName == AssemblyName;
        }
    }
}