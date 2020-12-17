namespace Soap.Bus
{
    using System.Reflection;

    public static class SoapBus
    {
        public static Assembly GetAssembly => Assembly.GetAssembly(typeof(Bus));
    }
}
