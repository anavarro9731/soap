namespace Soap.MessagePipeline.Context
{
    public interface IBootstrapVariables
    {
        AppEnvIdentifier AppEnvId { get; }

        string ApplicationName { get; set; }

        string ApplicationVersion { get; }

        string DefaultExceptionMessage { get; set; }

        bool ReturnExplicitErrorMessages { get; set; }
    }
}