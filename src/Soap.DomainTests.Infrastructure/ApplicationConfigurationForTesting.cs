namespace Soap.DomainTests.Infrastructure
{
    using Soap.Interfaces;

    public class ApplicationConfigurationForTesting : IApplicationConfig
    {
        public IApiEndpointSettings ApiEndpointSettings { get; set; } =
            MessagePipeline.Models.ApiEndpointSettings.Create("http://dev.Soap.http/endpoint/testing", "dev.service.api.msmq.endpoint.testing@machinename");

        public string ApplicationVersion { get; }

        public string ApplicationName { get; set; } = "Development Endpoint Tests";

        public string DefaultExceptionMessage { get; set; } = "Unexpected server error.";

        public string EnvironmentName { get; set; } = "Development";

        public byte NumberOfApiMessageRetries { get; set; } = 0;

        public bool ReturnExplicitErrorMessages { get; set; } = true;
    }
}