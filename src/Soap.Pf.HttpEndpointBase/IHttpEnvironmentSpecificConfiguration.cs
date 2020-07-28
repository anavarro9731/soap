namespace Soap.Pf.HttpEndpointBase
{
    public interface IHttpEnvironmentSpecificConfiguration : IEnvironmentSpecificConfig
    {
        void DefineCorsPolicyPerEnvironment(CorsPolicyBuilder policyBuilder);
    }
}