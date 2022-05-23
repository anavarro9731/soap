namespace Soap.Utility.Functions.Operations
{
    using System;
    using System.IO;
    using System.Reflection;
    using CircuitBoard;

    public static class Resources
    {
        public static byte[] ExtractResource(String filename, Assembly a)
        {
            return GetBytes(filename, a);
        }
        
        private static byte[] GetBytes(string filename, Assembly a)
        {
            string resourceName = $"{a.GetName().Name}.{filename}";
            using Stream resFilestream = a.GetManifestResourceStream(resourceName);
            if (resFilestream == null) throw new CircuitException($"Could not load embedded resource {resourceName}");
            byte[] ba = new byte[resFilestream.Length];
            resFilestream.Read(ba, 0, ba.Length);
            return ba;
        }
    }
}
