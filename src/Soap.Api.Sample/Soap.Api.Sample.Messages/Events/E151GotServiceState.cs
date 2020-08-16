namespace Sample.Messages.Events
{
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Binary;

    public class E151GotServiceState : ApiEvent
    {
        public override ApiPermission Permission { get; }

        public ServiceState State { get; set; }

        public class ServiceState
        {
            public Flags DatabaseState { get; set; }
        }
    }
}