namespace Soap.Interfaces
{
    public interface IConnectWithAuth0
    {
        public string Auth0TenantDomain { get; set; }
        public string Auth0EnterpriseAdminClientSecret { get; set; } 
        public string Auth0EnterpriseAdminClientId { get; set; } 
        public bool AuthEnabled { get; set; }
    }
}
