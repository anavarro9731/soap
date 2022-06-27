//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C100
{
    using System.Security;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Context.Exceptions;
    using Soap.Idaam;
    using Soap.Interfaces;
    using Soap.PfBase.Tests;
    using Soap.Utility;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC100v1WithAuthLevelApiAndDatabasePermission : Test
    {
        public TestC100v1WithAuthLevelApiAndDatabasePermission(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            
            
        }

        [Fact]
        public void ItShouldFailIfAuthIsEnabledAndTheUserDoesNotHavePermissionsToThisData()
        {
            TestMessage(Commands.Ping, Identities.WithApiPermissions, authLevel:AuthLevel.ApiAndDatabasePermission).Wait();
            
            Result.Success.Should().BeFalse();
            Result.UnhandledError.Should().BeOfType<FormattedExceptionInfo.PipelineException>();
            Result.ExceptionContainsErrorCode(DataStore.ErrorCodes.MissingDbPermissions);
            
        }
        
        [Fact]
        public void ItShouldSuccedIfAuthIsEnabledAndTheUserDoesHavePermissionsToThisData()
        {
            TestMessage(Commands.Ping, Identities.JohnDoeAllPermissions, authLevel:AuthLevel.ApiAndDatabasePermission).Wait();
            
            Result.Success.Should().BeTrue();

        }
    }
}
