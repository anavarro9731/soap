namespace Soap.Interfaces
{
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public class SoapEnvironments : Enumeration<SoapEnvironments>
    {
        public static SoapEnvironments Development = Create("DEV", nameof(Development));

        public static SoapEnvironments InMemory = Create("IN-MEM", nameof(InMemory));

        public static SoapEnvironments Live = Create("LIVE", nameof(Live));

        public static SoapEnvironments Release = Create("REL", nameof(Release));

        public static SoapEnvironments VNext = Create("VNEXT", nameof(VNext));
    }
}
