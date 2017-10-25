namespace Soap.Interfaces.Messages
{
    using System;

    public class ApiQuery : IApiQuery
    {
        public string IdentityToken { get; set; }

        public Guid MessageId { get; set; }

        public DateTime? TimeOfCreationAtOrigin { get; set; }
    }

    public class ApiQuery<T> : ApiQuery, IApiQuery<T> where T : class, new()
    {
        public T ReturnValue { get; set; }
    }
}