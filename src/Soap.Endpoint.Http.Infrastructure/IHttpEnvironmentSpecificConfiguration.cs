namespace Soap.Pf.HttpEndpointBase
{
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Soap.Pf.EndpointInfrastructure;

    public interface IHttpEnvironmentSpecificConfiguration : IEnvironmentSpecificConfig
    {
        void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder);
    }
}