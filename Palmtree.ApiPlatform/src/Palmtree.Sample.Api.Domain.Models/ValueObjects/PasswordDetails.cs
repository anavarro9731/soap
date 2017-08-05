namespace Palmtree.Sample.Api.Domain.Models.ValueObjects
{
    using System;

    public class PasswordDetails
    {
        public int HashIterations { get; set; }

        public string HexSalt { get; set; }

        public string PasswordHash { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }

        public string PasswordResetTokenHash { get; set; }

        public static PasswordDetails Create(
            int hashIterations,
            string hexSalt,
            string passwordHash,
            string passwordResetTokenHash = null,
            DateTime? passwordResetTokenExpiry = null)
        {
            return new PasswordDetails
            {
                HashIterations = hashIterations,
                HexSalt = hexSalt,
                PasswordHash = passwordHash,
                PasswordResetTokenHash = passwordResetTokenHash,
                PasswordResetTokenExpiry = passwordResetTokenExpiry
            };
        }
    }
}
