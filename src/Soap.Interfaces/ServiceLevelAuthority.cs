namespace Soap.Interfaces
{
    public class ServiceLevelAuthority
    {
        public string AppId;

        public string AccessToken;

        public string SlaIdChainSegment => "m2m://" + AppId;
        
        public ServiceLevelAuthority(string appId, string accessToken)
        {
            this.AppId = appId;
            this.AccessToken = accessToken;
        }
    }
}
