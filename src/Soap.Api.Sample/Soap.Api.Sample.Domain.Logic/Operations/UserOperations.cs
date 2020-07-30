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
    using Soap.Utility.Objects.Blended;

    public class UserOperations : Operations<User>
    {
        public Func<Task> AddBobaAndLando =>
            async () =>
                {
                {
                    await Validate();

                    DetermineChange(out var usersToAdd);

                    await Execute(usersToAdd);
                }

                async Task Validate()
                {
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
                    var names = newName.Split(' ');
                    var first = names[0];
                    var last = names[1];
                    nameChange = user =>
                        {
                        user.FirstName = first;
                        user.LastName = last;
                        };
                }

                async Task Execute(Guid id, Action<User> nameChange) => await DataWriter.UpdateById(id, nameChange);
                };
        
        public Func<Task> DeleteLukeSkywalker =>
            async () =>
                {
                {
                    Guid lukeId = Guid.Empty;

                    await Validate();

                    DetermineChange(v => lukeId = v);

                    await Execute(lukeId);
                }

                async Task Validate()
                {
                }

                async Task DetermineChange(Action<Guid> setLukeId)
                {
                    setLukeId((await DataReader.Read<User>(x => x.UserName == "luke.skywalker")).Single().id);
                }

                async Task Execute(Guid lukeId)
                {
                    await DataWriter.DeleteById<User>(lukeId, o => o.Permanently());
                }
                };

        public Func<Task> DeleteDarthVader =>
            async () =>
                {
                {
                    User darth = null;

                    await Validate();

                    DetermineChange(v => darth = v);

                    await Execute(darth);
                }

                async Task Validate()
                {
                }

                async Task DetermineChange(Action<User> setDarth)
                {
                    setDarth((await DataReader.Read<User>(x => x.UserName == "darth.vader")).Single());
                }

                async Task Execute(User darth)
                {
                    if (ContextWithMessageLogEntry.Current.Message.Headers.GetMessageId() == SpecialIds.RollbackHappyPath)
                        darth.Etag = "something that will fail this record";
                    await DataWriter.Delete(darth, o => o.Permanently());
                }
                };
        
        public Func<Task> ArchivePrincessLeia =>
            async () =>
                {
                {
                    User leia = null;

                    await Validate();

                    DetermineChange(v => leia = v);

                    await Execute(leia);
                }

                async Task Validate()
                {
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

        public class ErrorCodes : ErrorCode
        {
        }
    }
}