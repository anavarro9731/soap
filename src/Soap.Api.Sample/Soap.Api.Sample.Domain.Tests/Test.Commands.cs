namespace Sample.Tests
{
    using System;
    using Sample.Constants;
    using Sample.Messages.Commands;
    using Soap.Interfaces;

    public partial class Test
    {
        //all variables in here must remain constant for tests to be correct

        public static class Commands
        {
            public static C101UpgradeTheDatabase UpgradeTheDatabaseToV1 = new C101UpgradeTheDatabase(ReleaseVersions.V1);

            public static C101UpgradeTheDatabase UpgradeTheDatabaseToV2 = new C101UpgradeTheDatabase(ReleaseVersions.V2);

            public static C100Ping Ping = new C100Ping()
            {
                PingedAt = DateTime.UtcNow
            };
        }

        public static class Queries
        {
            public static C102GetServiceState GetServiceState = new C102GetServiceState();
        }
    }
}