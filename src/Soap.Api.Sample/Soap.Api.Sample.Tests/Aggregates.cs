//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Tests
{
    using Soap.Api.Sample.Models.Aggregates;

    /* all field values in here must remain constant for tests to be correct
     probably ok without arrows since datastore clones everything which allows you to use Guid.NewGuid() in Ids */

    public static class Aggregates
    {
        public static UserProfile DarthVader =>
            new UserProfile { UserName = "darth.vader", FirstName = "David", LastName = "Prowse", id = Ids.DarthVader, IdaamProviderId = Ids.DarthVader.ToIdaamId()};

        public static UserProfile HanSolo =>
            new UserProfile { UserName = "hans.solo", FirstName = "Harrison", LastName = "Ford", id = Ids.HanSolo, IdaamProviderId = Ids.HanSolo.ToIdaamId()};

        public static UserProfile LukeSkywalker =>
            new UserProfile { UserName = "luke.skywalker", FirstName = "Mark", LastName = "Hamill", id = Ids.LukeSkywalker, IdaamProviderId = Ids.LukeSkywalker.ToIdaamId()};

        public static UserProfile PrincessLeia =>
            new UserProfile { UserName = "leia.organa", FirstName = "Carrie", LastName = "Fisher", id = Ids.PrincessLeia, IdaamProviderId = Ids.PrincessLeia.ToIdaamId()};
    }
}
