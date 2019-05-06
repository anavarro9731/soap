﻿namespace Soap.Api.Sso.Domain.Models.ViewModels
{
    using System;
    using Soap.Api.Sso.Domain.Models.Aggregates;

    public class UserProfile
    {
        public string Email { get; private set; }

        public string FullName { get; private set; }

        public Guid id { get; private set; }

        public static UserProfile Create(User user)
        {
            return new UserProfile
            {
                Email = user.Email,
                FullName = user.FullName,
                id = user.id
            };
        }
    }
}