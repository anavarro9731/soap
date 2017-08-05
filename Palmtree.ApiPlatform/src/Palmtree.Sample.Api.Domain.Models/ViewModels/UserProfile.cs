namespace Palmtree.Sample.Api.Domain.Models.ViewModels
{
    using System;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;

    public class UserProfile
    {
        public string Email { get; private set; }

        public string FullName { get; private set; }

        public Guid Id { get; private set; }

        public static UserProfile Create(User user)
        {
            return new UserProfile
            {
                Email = user.Email,
                FullName = user.FullName,
                Id = user.id
            };
        }
    }
}
