//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.Mappings;
    using Soap.Context.MessageMapping;

    public partial class HandlerRegistration : MapMessagesToFunctions
    {
        public override void AddSpecial()
        {
            Register(new C104v1Functions());
            base.AddSpecial();
        }
    }
}
