namespace Sample.Logic.Operations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Models.PureFunctions;
    using Sample.Models.Aggregates;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Blended;

    public class UserOperations : Operations<User>
    {
        public Func<Task> AddTestUsers =>
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
                            UserName = "luke.skywalker"
                        },
                        new User
                        {
                            UserName = "leia.organa"
                        }
                    };
                }

                async Task Execute(User[] usersToAdd)
                {
                    foreach (var user in usersToAdd) DataWriter.Create(user);
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

                async Task Execute(Guid id, Action<User> nameChange) => DataWriter.UpdateById(id, nameChange);
                };

        public class ErrorCodes : ErrorCode
        {
        }

        public Func<Task> RemoveDarthVader =>
            async () =>
                {
                {
                    Guid darthId = Guid.Empty;
                    
                    await Validate();

                    DetermineChange(v => darthId = v);

                    await Execute(darthId);
                }

                async Task Validate()
                {
                }

                async Task DetermineChange(Action<Guid> setDarthId)
                {
                    setDarthId((await DataReader.Read<User>(x => x.UserName == "darth.vader")).Single().id);
                }

                async Task Execute(Guid darthId)
                {
                    DataWriter.DeleteById<User>(darthId, options => options.Permanently());
                }
                };
    }
}