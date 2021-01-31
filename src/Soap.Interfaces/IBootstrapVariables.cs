namespace Soap.Interfaces
{
    public interface IBootstrapVariables
    {
        bool UseServiceLevelAuthorityInTheAbsenceOfASecurityContext { get; set; }
        
        SoapEnvironments Environment { get; set; }

        string AppFriendlyName { get; set; }
        
        string AppId { get; set; }

        string ApplicationVersion { get; }

        string DefaultExceptionMessage { get; set; }
        
        bool AuthEnabled { get; set; }
        
        string EncryptionKey { get; set; }
    }
}
