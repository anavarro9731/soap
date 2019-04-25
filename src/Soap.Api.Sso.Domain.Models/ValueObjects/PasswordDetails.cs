namespace Soap.Api.Sso.Domain.Models.ValueObjects
{
    using System;

    public class PasswordDetails
    {
        public int HashIterations { get; set; }

        public string HexSalt { get; set; }

        public string PasswordHash { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }

        public Guid? PasswordResetStatefulProcessId { get; set; }

        public static PasswordDetails Create(
            int hashIterations,
            string hexSalt,
            string passwordHash,
            Guid? passwordResetStatefulProcessId = null,
            DateTime? passwordResetRequestExpiry = null)
        {
            return new PasswordDetails
            {
                HashIterations = hashIterations,
                HexSalt = hexSalt,
                PasswordHash = passwordHash,
                PasswordResetStatefulProcessId = passwordResetStatefulProcessId,
                PasswordResetTokenExpiry = passwordResetRequestExpiry
            };
        }
    }
}