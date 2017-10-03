namespace Palmtree.Api.Sso.Domain.Models.Entities
{
    using System;
    using Destructurama.Attributed;

    public class AccountHistory
    {
        public int InvalidLoginCount { get; set; }

        public DateTime? LastInvalidLoginAttempt { get; set; }

        public DateTime? LastLoggedIn { get; set; }

        [NotLogged]
        public DateTime? PasswordLastChanged { get; set; }

        public static AccountHistory Create()
        {
            return new AccountHistory();
        }
    }
}
