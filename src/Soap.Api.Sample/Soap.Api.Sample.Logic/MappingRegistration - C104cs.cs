//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.Mappings;
    using Soap.Context.MessageMapping;

    public partial class MappingRegistration : MapMessagesToFunctions
    {
        public override void AddSpecial()
        {
            Register(new C104Mapping());
            base.AddSpecial();
        }
    }
}