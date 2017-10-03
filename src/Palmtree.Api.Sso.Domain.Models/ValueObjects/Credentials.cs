namespace Palmtree.Api.Sso.Domain.Models.ValueObjects
{
    public class Credentials
    {
        private Credentials()
        {
        }

        public string Password { get; set; }

        public string Username { get; set; }

        public static Credentials Create(string username, string password)
        {
            return new Credentials
            {
                Username = username,
                Password = password
            };
        }
    }
}
