namespace Soap.Api.Sample.Messages.Events
{
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class E151v1_GotServiceState : ApiEvent
    {
        public ServiceState E151_State { get; set; }

        public class ServiceState
        {
            public EnumerationFlags E151_DatabaseState { get; set; }
        }

        public override void Validate()
        {
            
        }
    }
}
