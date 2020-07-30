namespace Sample.Tests
{
    using Sample.Models.Aggregates;

    /* all field values in here must remain constant for tests to be correct
     probably ok without arrows since datastore clones everything which allows you to use Guid.NewGuid() in Ids */

    public static class Aggregates
    {
        public static User DarthVader =>
            new User { UserName = "darth.vader", FirstName = "David", LastName = "Prowse", id = Ids.DarthVader };

        public static User HanSolo =>
            new User { UserName = "hans.solo", FirstName = "Alec", LastName = "Guiness", id = Ids.HanSolo };

        public static User LukeSkywalker =>
            new User { UserName = "luke.skywalker", FirstName = "Mark", LastName = "Hamill", id = Ids.LukeSkywalker };

        public static User PrincessLeia =>
            new User { UserName = "leia.organa", FirstName = "Carrie", LastName = "Fisher", id = Ids.PrincessLeia };
    }
}