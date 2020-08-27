namespace Soap.MessagePipeline.Context
{
    using Soap.Utility.Objects.Blended;

    public class SoapEnvironments : Enumeration<SoapEnvironments>
    {
        public static SoapEnvironments InMemory = Create(nameof(InMemory), "IN-MEM");
        
        public static SoapEnvironments Development = Create(nameof(Development), "DEV");

        public static SoapEnvironments VNext = Create(nameof(VNext), "VNEXT");

        public static SoapEnvironments Release = Create(nameof(Release), "REL");
        
        public static SoapEnvironments Live = Create(nameof(Live), "LIVE");
    }
}