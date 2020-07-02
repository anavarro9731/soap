namespace Sample.Tests
{
    using Sample.Messages.Commands;
    using Sample.Models.Constants;

    public partial class Test
    {
        //all variables in here must remain constant for tests to be correct

        public static class Commands
        {
            public static UpgradeTheDatabaseCommand UpgradeTheDatabaseToV1 = new UpgradeTheDatabaseCommand(ReleaseVersions.V1);
        }
    }
}