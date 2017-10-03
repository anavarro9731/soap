namespace Soap.DomainTests.Infrastructure
{
    using Soap.Interfaces;

    public class ApplicationConfigurationForTesting : IApplicationConfig
    {
        public IApiServerSettings ApiServerSettings { get; set; } = MessagePipeline.Models.ApiServerSettings.Create(
            "http://dev.Soap.http/endpoint/testing",
            "dev.service.api.msmq.endpoint.testing@machinename");

        public string ApplicationName { get; set; } = "Development Endpoint Tests";

        public string DefaultExceptionMessage { get; set; } = "Unexpected server error.";

        public string EnvironmentName { get; set; } = "Development";

        public byte NumberOfApiMessageRetries { get; set; } = 0;

        public bool ReturnExplicitErrorMessages { get; set; } = true;
    }
}
