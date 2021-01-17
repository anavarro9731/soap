namespace Soap.Interfaces
{
    public interface IConnectWithAuth0
    {
        public string Auth0ManagementApiUri { get; set; }
        public string Auth0TokenEndpointUri { get; set; } 
        public string Auth0HealthCheckClientSecret { get; set; } 
        public string Auth0HealthCheckClientId { get; set; } 
        public bool Auth0Enabled { get; set; }
    }
}
