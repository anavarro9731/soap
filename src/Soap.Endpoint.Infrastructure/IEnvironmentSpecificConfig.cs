namespace Soap.Endpoint.Infrastructure
{
    using Serilog;
    using Soap.Interfaces;

    public interface IEnvironmentSpecificConfig
    {
        IApplicationConfig Variables { get; }

        void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration logger);
    }
}