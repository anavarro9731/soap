namespace Soap.Auth0
{
    using System.Reflection;

    public static class SoapAuth
    {
        public static Assembly GetAssembly => Assembly.GetAssembly(typeof(Auth0Functions));
    }
}
