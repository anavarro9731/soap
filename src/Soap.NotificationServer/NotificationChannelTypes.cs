namespace Soap.NotificationServer
{
    using Soap.Utility.Objects.Blended;

    public class NotificationChannelTypes : Enumeration<NotificationChannelTypes>
    {
        public static NotificationChannelTypes Email = Create(nameof(Email), "Email");

        public static NotificationChannelTypes InMemory = Create(nameof(InMemory), "InMemory");
    }
}