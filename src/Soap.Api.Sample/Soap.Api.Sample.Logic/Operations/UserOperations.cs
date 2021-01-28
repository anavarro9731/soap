//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Operations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;
    using Soap.Context.Context;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Guard = Soap.Context.Guard;

    /// <summary>
    ///     Not really representative of user operations, too specific, uses undocumented features for testing UOW
    /// </summary>
    public class UserOperations : Operations<UserProfile>
    {
        
        public Func<Task> AddBobaAndLando =>
            async () =>
                {
                {
                    DetermineChange(out var usersToAdd);

                    await Execute(usersToAdd);
                }

                void DetermineChange(out UserProfile[] usersToAdd)
                {
                    usersToAdd = new[]
                    {
                        new UserProfile
                        {
                            UserName = "boba.fett"
                        },
                        new UserProfile
                        {
                            UserName = "lando.calrissian"
                        }
                    };
                }

                async Task Execute(UserProfile[] usersToAdd)
                {
                    foreach (var user in usersToAdd) await DataWriter.Create(user);
                }
                };

        public Func<Task> AddR2D2AndC3PO =>
            async () =>
                {
                {
                    DetermineChange(out var usersToAdd);

                    await Execute(usersToAdd);
                }

                void DetermineChange(out UserProfile[] usersToAdd)
                {
                    usersToAdd = new[]
                    {
                        new UserProfile
                        {
                            UserName = "r2d2"
                        },
                        new UserProfile
                        {
                            UserName = "c3po"
                        }
                    };
                }

                async Task Execute(UserProfile[] usersToAdd)
                {
                    foreach (var user in usersToAdd) await DataWriter.Create(user);
                }
                };

        public Func<Task> ArchivePrincessLeia =>
            async () =>
                {
                {
                    UserProfile leia = null;

                    await DetermineChange(v => leia = v);

                    await Execute(leia);
                }

                async Task DetermineChange(Action<UserProfile> setLeia)
                {
                    setLeia((await DataReader.Read<UserProfile>(x => x.UserName == "leia.organa")).Single());
                }

                async Task Execute(UserProfile leia)
                {
                    await DataWriter.Delete(leia);
                }
                };

        public Func<string, Task> ChangeHansSoloName =>
            async newName =>
                {
                {
                    UserProfile hanSolo = null;

                    await Validate(newName, v => hanSolo = v);

                    DetermineChange(out var changeName);

                    await Execute(hanSolo.id, changeName);
                }

                async Task Validate(string newName, Action<UserProfile> setHanSolo)
                {
                    Guard.Against(newName.Split(' ').Length != 2, "Name must be of format \"First Last\"");

                    var hansSolo = (await DataReader.ReadActive<UserProfile>(u => u.UserName == "hans.solo")).Single();

                    Guard.Against(hansSolo == null, "Hans Solo doesn't exist");

                    setHanSolo(hansSolo);
                }

                void DetermineChange(out Action<UserProfile> nameChange)
                {
                    var currentMessageId = ContextWithMessageLogEntry.Current.Message.Headers.GetMessageId();
                    var currentMessageLogEntry = ContextWithMessageLogEntry.Current.MessageLogEntry;

                    nameChange = user =>
                        {
                        if (currentMessageId == SpecialIds.ProcessesSomeDataButThenFailsCompletesSuccessfullyOnRetry
                            && currentMessageLogEntry.Attempts.Count == 0)
                        {
                            user.Etag = "123456";
                        }
                        else
                        {
                            var names = newName.Split(' ');
                            var first = names[0];
                            var last = names[1];
                            user.FirstName = first;
                            user.LastName = last;
                        }
                        };
                }

                async Task Execute(Guid id, Action<UserProfile> nameChange) => await DataWriter.UpdateById(id, nameChange);
                };

        public Func<Task> ChangeLukeSkywalkersName =>
            async () =>
                {
                {
                    var lukeId = Guid.Empty;

                    await Validate(v => lukeId = v);

                    DetermineChange(out var changeLuke);

                    await Execute(lukeId, changeLuke);
                }

                async Task Validate(Action<Guid> setLukeId)
                {
                    setLukeId((await DataReader.Read<UserProfile>(x => x.UserName == "luke.skywalker")).Single().id);
                }

                void DetermineChange(out Action<UserProfile> changeLuke)
                {
                    var currentMessageId = ContextWithMessageLogEntry.Current.Message.Headers.GetMessageId();

                    changeLuke = luke => luke.LastName = "Spywalker";

                    switch (currentMessageId)
                    {
                        case var a when a == SpecialIds.ProcessesSomeThenRollsBackSuccessfully:
                        case var b when b == SpecialIds.ConsideredAsRolledBackWhenFirstItemFails:
                        case var c when c == SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry:
                        case var d when d == SpecialIds.FailsEarlyInReplayThenCompletesRemainderOfUow:
                        case var e when e == SpecialIds.RollbackSkipsOverItemsDeletedSinceWeChangedThem:
                        case var f when f == SpecialIds.RollbackSkipsOverItemsUpdatedAfterWeUpdatedThem:
                        case var g when g == SpecialIds.RollbackSkipsOverItemsDeletedSinceWeCreatedThem:
                        case var h when h == SpecialIds.RollbackSkipsOverItemsUpdatedSinceWeCreatedThem:
                        case var i when i == SpecialIds.ShouldFailOnEtagButWithOptimisticConcurrencyOffItSucceeds:
                            
                            if (ContextWithMessageLogEntry.Current.MessageLogEntry.Attempts.Count == 0)
                            {
                                /* causes dbconcurrency exception on first attempt luke's record never commits
                                in reality there is a tiny window between loading a saving an update so the eTag
                                violation we are simulating here is pretty much impossible to trigger in a test
                                the important par    t is we have to be sure in the body of the test to change the underlying record
                                to reflect an outside change before the uow is retried so it will give up and not
                                try to reprocess the message which would just keep erroring */
                                changeLuke = luke => luke.Etag = "something that breaks";
                            }
                            break;
                    }
                }

                async Task Execute(Guid lukeId, Action<UserProfile> changeLuke)
                {
                    await DataWriter.UpdateById(lukeId, changeLuke);
                }
                };

        public Func<Task> DeleteDarthVader =>
            async () =>
                {
                {
                    UserProfile darth = null;

                    await DetermineChange(v => darth = v);

                    await Execute(darth);
                }

                async Task DetermineChange(Action<UserProfile> setDarth)
                {
                    setDarth((await DataReader.Read<UserProfile>(x => x.UserName == "darth.vader")).Single());
                }

                async Task Execute(UserProfile darth)
                {
                    await DataWriter.Delete(darth, o => o.Permanently());
                }
                };
    }
}
