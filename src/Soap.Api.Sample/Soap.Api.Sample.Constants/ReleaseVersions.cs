namespace Soap.Api.Sample.Constants
{
    using CircuitBoard;

    public class ReleaseVersions : TypedEnumeration<ReleaseVersions>
    {
        public static ReleaseVersions V1 = Create("1", "Version 1");

        public static ReleaseVersions V2 = Create("2", "Version 2");
    }
}
