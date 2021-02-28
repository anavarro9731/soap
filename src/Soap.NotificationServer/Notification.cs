namespace Soap.NotificationServer
{
    using FluentValidation;

    public class Notification 
    {
        public Notification(string subject, string body)
        {
            this.Subject = subject;
            this.Body = body;
        }
        
        public string Body;

        public string Subject;

        public class Validator : AbstractValidator<Notification>
        {
            public Validator()
            {
                RuleFor(x => x.Subject).NotEmpty();
                RuleFor(x => x.Body).NotEmpty();
            }
        }
    }
}
