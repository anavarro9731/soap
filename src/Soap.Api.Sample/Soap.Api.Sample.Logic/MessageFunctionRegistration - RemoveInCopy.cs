//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.MessageFunctions;
    using Soap.Context.MessageMapping;

    public partial class MessageFunctionRegistration : MapMessagesToFunctions
    {
        protected override void RegisterOnlyForSampleApi()
        {
            Register(new C103v1Functions());
            Register(new C104v1Functions());
            Register(new C107v1Functions());
            Register(new C109v1Functions());
            Register(new C110v1Functions());
            Register(new C111v1Functions());
            Register(new C111v2Functions());
            Register(new C112v1Functions());
            Register(new C113v1Functions());
            Register(new C114v1Functions());
        }
    }
}
