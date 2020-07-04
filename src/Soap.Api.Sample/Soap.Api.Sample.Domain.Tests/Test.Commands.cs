namespace Sample.Tests
{
    using Sample.Messages.Commands;
    using Sample.Messages.Queries;
    using Sample.Models.Constants;
    using Soap.Interfaces;

    public partial class Test
    {
        //all variables in here must remain constant for tests to be correct

        public static class Commands
        {
            public static UpgradeTheDatabaseCommand UpgradeTheDatabaseToV1 = new UpgradeTheDatabaseCommand(ReleaseVersions.V1);
            public static UpgradeTheDatabaseCommand UpgradeTheDatabaseToV2 = new UpgradeTheDatabaseCommand(ReleaseVersions.V2);
        }

        public static class Queries
        {
            public static GetMessageLogItemQuery GetMessageLogItem = new GetMessageLogItemQuery();
        }
    }
}