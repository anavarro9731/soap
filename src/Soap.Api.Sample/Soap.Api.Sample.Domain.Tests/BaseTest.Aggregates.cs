namespace Sample.Tests
{
    using DataStore.Interfaces.LowLevel;
    using Sample.Models.Aggregates;

    public partial class BaseTest
    {
        //all variables in here must remain constant for tests to be correct

        protected static class Aggregates
        {
            public static User HansSolo => new User() { UserName = "hans.solo", FirstName = "Alec", LastName = "Guiness"};
            public static User DarthVader => new User() { UserName = "darth.vader", FirstName = "David", LastName = "Prowse"};
        }
    }
}