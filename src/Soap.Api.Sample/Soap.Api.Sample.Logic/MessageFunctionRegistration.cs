namespace Soap.Api.Sample.Logic
{
    using Soap.Api.Sample.Logic.MessageFunctions;
    using Soap.Context.MessageMapping;

    public partial class MessageFunctionRegistration : MapMessagesToFunctions
    {
        public MessageFunctionRegistration()
        {
            Register(new C100v1Functions());
            Register(new C101v1Functions());
            Register(new C102v1Functions());
            Register(new C105v1Functions());
            Register(new C106v1Functions());
            Register(new C115v1Functions());
            Register(new E100v1Functions());
            Register(new C116v1Functions());
            /* ##NEXT## */
        }
    }
}
