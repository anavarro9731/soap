namespace Soap.Interfaces
{
    public interface IBootstrapVariables
    {
        SoapEnvironments Environment { get; set; }

        string AppFriendlyName { get; set; }
        
        string AppId { get; set; }

        string ApplicationVersion { get; }

        string DefaultExceptionMessage { get; set; }

        bool ReturnExplicitErrorMessages { get; set; }
    }
}