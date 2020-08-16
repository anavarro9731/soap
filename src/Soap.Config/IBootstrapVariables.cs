namespace Soap.MessagePipeline.Context
{
    public interface IBootstrapVariables
    {
        string ApplicationName { get; set; }

        string ApplicationVersion { get; set; }

        string DefaultExceptionMessage { get; set; }

        string EnvironmentName { get; set; }

        bool ReturnExplicitErrorMessages { get; set; }
    }
}