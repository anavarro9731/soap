namespace Soap.Interfaces
{
    public interface IApplicationConfig
    {
        IApiServerSettings ApiServerSettings { get; }

        string ApplicationName { get; }

        string DefaultExceptionMessage { get; }

        string EnvironmentName { get; }

        byte NumberOfApiMessageRetries { get; }

        bool ReturnExplicitErrorMessages { get; }
    }
}