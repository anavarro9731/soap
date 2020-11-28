namespace Soap.Api.Sample.Constants
{
    using CircuitBoard;

    public class ReleaseVersions : TypedEnumeration<ReleaseVersions>
    {
        public static ReleaseVersions V1 = Create("v1", "Version 1");

        public static ReleaseVersions V2 = Create("v2", "Version 2");
    }
}
