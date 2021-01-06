namespace Soap.Api.Sample.Tests
{
    using System;
    using Soap.Api.Sample.Constants;
    using Soap.Api.Sample.Messages.Commands;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share message instances and have concurrency problems otherwise */

    public static partial class Commands
    {
        public static C100v1_Ping Ping =>
            new C100v1_Ping
            {
                C000_PingedAt = DateTime.UtcNow
            };

        public static C101v1_UpgradeTheDatabase UpgradeTheDatabaseToV1 => new C101v1_UpgradeTheDatabase(ReleaseVersions.V1);

        public static C101v1_UpgradeTheDatabase UpgradeTheDatabaseToV2 => new C101v1_UpgradeTheDatabase(ReleaseVersions.V2);
    }
}
