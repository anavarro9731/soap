namespace Soap.If.Utility.PureFunctions.Extensions
{
    using System.Security;

    public static class SecureStringExt
    {
        public static SecureString ToSecureString(this string unsecuredString)
        {
            var results = new SecureString();
            foreach (var ch in unsecuredString) results.AppendChar(ch);

            return results;
        }
    }
}
