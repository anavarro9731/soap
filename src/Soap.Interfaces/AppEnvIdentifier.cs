namespace Soap.Interfaces
{
    public class AppEnvIdentifier
    {
        public string AppKey;

        public string EnvironmentKey;

        public AppEnvIdentifier(string appKey, SoapEnvironments environment)
        {
            this.AppKey = appKey;
            this.EnvironmentKey = environment.Key;
        }

        public override string ToString() => this.AppKey + this.EnvironmentKey;
    }
}