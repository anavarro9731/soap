// TODO
// namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
// {
//     using global::Sample.Messages.Commands;
//     using global::Sample.Models.Constants;
//     using Xunit;
//
//     public class WhenUpgradingTheDatabaseMoreThanOnce : Test
//     {
//         public WhenUpgradingTheDatabaseMoreThanOnce()
//         {
//             this.endPoint.HandleCommand(new UpgradeTheDatabaseCommand(ReleaseVersions.v1));
//
//             this.endPoint.HandleCommand(new UpgradeTheDatabaseCommand(ReleaseVersions.v2));
//         }
//
//         [Fact]
//         public void ItShouldSetTheServiceStateDbVersionTo2()
//         {
//             var ss = this.endPoint.QueryDatabase<ServiceState>().Result.Single();
//             ss.DatabaseState.HasState(ReleaseVersions.v2).Should().BeTrue();
//         }
//     }
// }