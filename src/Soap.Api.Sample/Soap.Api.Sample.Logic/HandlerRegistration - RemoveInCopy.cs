//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.Handlers;
    using Soap.Context.MessageMapping;

    public partial class HandlerRegistration : MapMessagesToFunctions
    {
        protected override void RegisterOnlyForSampleApi()
        {
            Register(new C102v1Functions());
            Register(new C104v1Functions());
        }
    }
}
