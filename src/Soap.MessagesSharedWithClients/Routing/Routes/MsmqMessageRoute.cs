namespace Soap.Pf.ClientServerMessaging.Routing.Routes
{
    using System;
    using System.Collections.Generic;
    using Soap.Pf.ClientServerMessaging.Routing.Addresses;

    public abstract class MsmqMessageRoute : MessageRoute
    {
        public MsmqEndpointAddress MsmqEndpointAddress { get; set; }

        protected override string To => MsmqEndpointAddress.ToString();

        public List<Type> MessageTypes { get; } = new List<Type>(); 
    }
}