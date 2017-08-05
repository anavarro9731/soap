namespace Palmtree.ApiPlatform.Endpoint.Infrastructure
{
    using Serilog;
    using Palmtree.ApiPlatform.Interfaces;

    public interface IEnvironmentSpecificConfig
    {
        IApplicationConfig Variables { get; }

        void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration logger);
    }
}
