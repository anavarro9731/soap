namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P205_C100__RespondToPing : Process, IBeginProcess<C100v1_Ping>
    {
        public Func<C100v1_Ping, Task> BeginProcess =>
            async message =>
                {
                {
                    var pongedBy = Meta.UserProfileOrNull?.IdaamProviderId;
                    var isServiceIdentity = message.Headers.GetIdentityChain().StartsWith("service-identity") && Meta.UserProfileOrNull == null;
                    if (Meta.AuthLevel.DatabasePermissionEnabled && !isServiceIdentity)
                    {
                        var myProfile = Meta.AuthLevel switch
                        {
                            { } when Meta.AuthLevel == AuthLevel.ApiAndDatabasePermission => (await DataReader.Read<UserProfile>(
                                                                                                  x => x.IdaamProviderId == pongedBy,
                                                                                                  op => op.AuthoriseFor(Meta.IdentityClaimsOrNull)))
                                .Single(),

                            { } when Meta.AuthLevel == AuthLevel.ApiAndAutoDbAuth => (await DataReader.Read<UserProfile>(
                                                                                          x => x.IdaamProviderId == pongedBy)).Single(),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        pongedBy = myProfile.IdaamProviderId;
                    }

                    await PublishPong(pongedBy);
                }

                async Task PublishPong(string pongedBy)
                {
                    await Bus.Publish(
                        new E100v1_Pong
                        {
                            E000_PingedAt = message.C000_PingedAt,
                            E000_PingedBy = message.C000_PingedBy,
                            E000_PongedAt = DateTime.UtcNow,
                            E000_PongedBy = pongedBy
                        });
                }
                };
    }
}