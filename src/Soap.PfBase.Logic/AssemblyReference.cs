namespace Soap.PfBase.Logic
{
    using System.Reflection;

    public static class SoapPfLogicBase
    {
        public static Assembly GetAssembly => Assembly.GetAssembly(typeof(SoapPfLogicBase));
    }
}