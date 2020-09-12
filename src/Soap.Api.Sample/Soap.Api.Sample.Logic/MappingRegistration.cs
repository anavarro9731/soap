namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.Mappings;
    using Soap.Context.MessageMapping;

    public class MappingRegistration : MapMessagesToFunctions
    {
        public MappingRegistration()
        {
            Register(new C100Mapping());
            Register(new C101Mapping());
            Register(new C102Mapping());
            Register(new C103Mapping());
            Register(new C104Mapping());
            Register(new E150Mapping());
        }
    }
}