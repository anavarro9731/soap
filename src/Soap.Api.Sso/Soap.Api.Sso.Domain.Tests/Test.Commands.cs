namespace Soap.Api.Sso.Domain.Tests
{
    using System;
    using Soap.Api.Sso.Domain.Messages.Commands;

    public partial class Test
    {
        //all variables in here must remain constant for tests to be correct

        public static class Commands
        {
            public static AddFullyRegisteredUser CreateRegisteredUser1(
                string email = "admiralackbar@rebelalliance.org",
                string name = "Adm. Ackbar",
                string password = "itsatrap")
            {
                return new AddFullyRegisteredUser(Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4"), email, name, password);
            }

            public static AddFullyRegisteredUser CreateRegisteredUser2(
                string email = "monmotha @rebelalliance.org",
                string name = "Mon Monthma",
                string password = "rebels4ever")
            {
                return new AddFullyRegisteredUser(Guid.Parse("e22449a0-4ecc-4aec-b3de-dc5f8e9acb9e"), email, name, password);
            }
        }
    }
}