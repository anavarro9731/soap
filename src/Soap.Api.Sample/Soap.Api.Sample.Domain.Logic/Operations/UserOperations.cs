namespace Sample.Logic.Operations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Models.PureFunctions;
    using Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.MessagePipeline;
    using Soap.MessagePipeline.Context;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class UserOperations : Operations<User>
    {
        
        public Func<Task> AddBobaAndLando =>
            async () =>
                {
                {
                    DetermineChange(out var usersToAdd);

                    await Execute(usersToAdd);
                }

                void DetermineChange(out User[] usersToAdd)
                {
                    usersToAdd = new[]
                    {
                        new User
                        {
                            UserName = "boba.fett"
                        },
                        new User
                        {
                            UserName = "lando.calrissian"
                        }
                    };
                }

                async Task Execute(User[] usersToAdd)
                {
                    foreach (var user in usersToAdd) await DataWriter.Create(user);
                }
                };

        public Func<Task> ArchivePrincessLeia =>
            async () =>
                {
                {
                    User leia = null;

                    await DetermineChange(v => leia = v);

                    await Execute(leia);
                }

                async Task DetermineChange(Action<User> setLeia)
                {
                    setLeia((await DataReader.Read<User>(x => x.UserName == "leia.organa")).Single());
                }

                async Task Execute(User leia)
                {
                    await DataWriter.Delete(leia);
                }
                };

        public Func<string, Task> ChangeHansSoloName =>
            async newName =>
                {
                {
                    User hanSolo = null;

                    await Validate(newName, v => hanSolo = v);

                    DetermineChange(out var changeName);

                    await Execute(hanSolo.id, changeName);
                }

                async Task Validate(string newName, Action<User> setHanSolo)
                {
                    Guard.Against(newName.Split(' ').Length != 2, "Name must be of format \"First Last\"");

                    var hansSolo = (await DataReader.ReadActive<User>(u => u.UserName == "hans.solo")).Single();

                    Guard.Against(hansSolo == null, "Hans Solo doesn't exist");

                    setHanSolo(hansSolo);
                }

                void DetermineChange(out Action<User> nameChange)
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

                async Task Execute(Guid id, Action<User> nameChange) => await DataWriter.UpdateById(id, nameChange);
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
                    setLukeId((await DataReader.Read<User>(x => x.UserName == "luke.skywalker")).Single().id);
                }

                void DetermineChange(out Action<User> changeLuke)
                {
                    var currentMessageId = ContextWithMessageLogEntry.Current.Message.Headers.GetMessageId();

                    if (currentMessageId == SpecialIds.ProcessesSomeThenRollsBackSuccessfully || currentMessageId == SpecialIds.ConsideredAsRolledBackWhenFirstItemFails ||
                        currentMessageId == SpecialIds.FailsDuringRollbackFinishesRollbackOnNextRetry
                        && ContextWithMessageLogEntry.Current.MessageLogEntry.Attempts.Count == 0)
                    {
                        /* causes dbconcurrency exception on first attempt luke's record never commits
                        in reality there is a tiny window between loading a saving an update so the eTag
                        violation we are simulating here is pretty much impossible to trigger in a test
                        the important part is we have to be sure in the body of the test to change the underlying record
                        to reflect an outside change before the uow is retried so it will give up and not
                        try to reprocess the message which would just keep erroring */
                        changeLuke = luke => luke.Etag = "something that breaks";
                    }
                    else
                    {
                        changeLuke = luke => luke.LastName = "Spywalker";
                    }
                }

                async Task Execute(Guid lukeId, Action<User> changeLuke)
                {
                    await DataWriter.UpdateById(lukeId, changeLuke);
                }
                };

        public Func<Task> DeleteDarthVader =>
            async () =>
                {
                {
                    User darth = null;

                    await DetermineChange(v => darth = v);

                    await Execute(darth);
                }

                async Task DetermineChange(Action<User> setDarth)
                {
                    setDarth((await DataReader.Read<User>(x => x.UserName == "darth.vader")).Single());
                }

                async Task Execute(User darth)
                {
                    await DataWriter.Delete(darth, o => o.Permanently());
                }
                };
    }
}