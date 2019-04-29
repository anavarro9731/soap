namespace Soap.Pf.EndpointInfrastructure
{
    using Serilog;
    using Soap.If.Interfaces;

    public interface IEnvironmentSpecificConfig
    {
        IApplicationConfig Variables { get; }

        void DefineLoggingPolicyPerEnvironment(out LoggerConfiguration logger);
    }
}