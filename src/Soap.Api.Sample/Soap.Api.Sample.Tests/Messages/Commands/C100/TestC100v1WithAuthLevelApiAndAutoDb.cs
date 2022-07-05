//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C100
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Options;
    using FluentAssertions;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context.Exceptions;
    using Soap.Interfaces;
    using Soap.PfBase.Tests;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC100v1WithAuthLevelApiAndAutoDb : Test
    {
        private readonly Guid tenantId = Guid.NewGuid();

        public TestC100v1WithAuthLevelApiAndAutoDb(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ItShouldFailIfAuthIsEnabledAndTheUserDoesNotHavePermissionsToThisData()
        {
            TestMessage(Commands.Ping, Identities.WithApiPermissions, authLevel: AuthLevel.AuthoriseApiAndDatabasePermissionsOptOut).Wait();

            Result.Success.Should().BeFalse();
            Result.UnhandledError.Should().BeOfType<FormattedExceptionInfo.PipelineException>();
            Result.ExceptionContainsErrorCode(ErrorCodes.MissingDbPermissions);
        }

        [Fact]
        public void ItShouldSucceedIfAuthIsEnabledAndTheUserDoesHavePermissionsToThisData()
        {
            Setup();

            var identityToUse = Identities.WithApiPermissions;

            TestMessage(
                    Commands.Ping,
                    identityToUse,
                    authLevel: AuthLevel.AuthoriseApiAndDatabasePermissionsOptOut,
                    beforeRunHook: (args =>
                                           {
                                           
                                           var tenantReference = new AggregateReference(this.tenantId, typeof(Tenant).FullName);

                                           foreach (var roleInstance in identityToUse.Roles)
                                               args.IdaamProvider.AddScopeToUserRole(
                                                   identityToUse.UserProfile.IdaamProviderId,
                                                   new SecurityInfo().BuiltInRoles.Single(x => x.Key == roleInstance.RoleKey),
                                                   tenantReference);

                                           return Task.CompletedTask;
                                           }, default))
                .Wait();

            Result.Success.Should().BeTrue();
        }

        private void Setup()
        {
            //* add tenant
            SetupTestByAddingADatabaseEntry(
                new Tenant
                {
                    id = this.tenantId
                });

            //* add the user profile we will look for in P205 scoped to the tenant
            var userProfile = Identities.WithApiPermissions.UserProfile.As<UserProfile>().Op(p => p.TenantId = this.tenantId);

            SetupTestByAddingADatabaseEntry(userProfile);
        }
    }
}