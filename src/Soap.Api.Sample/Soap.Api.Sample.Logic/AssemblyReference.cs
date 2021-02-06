namespace Soap.Api.Sample.Logic
{
    using System.Reflection;

    public static class SoapApiSampleLogic
    {
        public static Assembly GetAssembly => Assembly.GetAssembly(typeof(MessageFunctionRegistration));
    }
}
