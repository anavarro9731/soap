namespace Soap.Idaam
{
    using System.Reflection;

    public static class SoapAuth
    {
        public static Assembly GetAssembly => Assembly.GetAssembly(typeof(IdaamProvider));
    }
}
