namespace Soap.Api.Sample.Messages.Events
{
    using Soap.Interfaces.Messages;

    public class E151GotServiceState : ApiEvent
    {
        public override ApiPermission Permission { get; }

        public ServiceState State { get; set; }

        public class ServiceState
        {
            public EnumerationFlags DatabaseState { get; set; }
        }
    }
}