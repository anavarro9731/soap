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
        public static C102v1GetServiceState GetServiceState => new C102v1GetServiceState();

        public static C100v1Ping Ping =>
            new C100v1Ping
            {
                PingedAt = DateTime.UtcNow
            };

        public static C101v1UpgradeTheDatabase UpgradeTheDatabaseToV1 => new C101v1UpgradeTheDatabase(C101v1UpgradeTheDatabase.ReleaseVersions.V1);

        public static C101v1UpgradeTheDatabase UpgradeTheDatabaseToV2 => new C101v1UpgradeTheDatabase(C101v1UpgradeTheDatabase.ReleaseVersions.V2);
        
    }
}