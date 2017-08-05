namespace Palmtree.Sample.Api.Domain.Models.ViewModels
{
    using System;

    public class RegistrationResult
    {
        public string Message { get; set; }

        public Guid ProcessId { get; set; }

        public bool Success { get; set; }

        public UserProfile User { get; set; }

        public static RegistrationResult Create(string message, UserProfile user, bool success, Guid processId)
        {
            return new RegistrationResult
            {
                Message = message,
                User = user,
                Success = success,
                ProcessId = processId
            };
        }
    }
}
