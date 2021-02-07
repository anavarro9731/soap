namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.MessageFunctions;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Context.MessageMapping;

    public partial class MessageFunctionRegistration : MapMessagesToFunctions
    {
        public MessageFunctionRegistration()
        {
            Register(new C100v1Functions());
            Register(new C101v1Functions());
            Register(new C103v1Functions());
            Register(new C105v1Functions());
            Register(new C106v1Functions());
            Register(new E100v1Functions());
            Register(new C107v1Functions());
            Register(new C109v1Functions());
            Register(new C110v1Functions());
            Register(new C111v1Functions());
            Register(new C111v2Functions());
            Register(new C113v1Functions());
            Register(new C114v1Functions());
        }
    }
}
