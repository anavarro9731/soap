namespace Sample.Logic
{
    using Sample.Logic.Mappers;
    using Soap.MessagePipeline.MessagePipeline;

    public class Mappings : MapMessagesToFunctions
    {
        public Mappings()
        {
            AddMapping(new UpgradeDatabaseMapping());
        }
    }
}