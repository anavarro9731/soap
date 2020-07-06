namespace Sample.Messages.Events
{
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Binary;

    public class E151GotServiceState : ApiEvent
    {
        public ServiceState State { get; set; } 

        public class ServiceState
        {
            public Flags DatabaseState { get; set; }
        }

        public override ApiPermission Permission { get; }
    }
}