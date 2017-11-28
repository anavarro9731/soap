namespace Soap.Interfaces
{
    public interface IApplicationConfig
    {
        IApiEndpointSettings ApiEndpointSettings { get; }

        string ApplicationVersion { get; }

        string ApplicationName { get; }

        string DefaultExceptionMessage { get; }

        string EnvironmentName { get; }

        byte NumberOfApiMessageRetries { get; }

        bool ReturnExplicitErrorMessages { get; }
    }
}