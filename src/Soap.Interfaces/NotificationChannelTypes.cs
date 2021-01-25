namespace Soap.Interfaces
{
    using CircuitBoard;

    public class NotificationChannelTypes : TypedEnumeration<NotificationChannelTypes>
    {
        public static NotificationChannelTypes Email = Create(nameof(Email), "Email");

        public static NotificationChannelTypes InMemory = Create(nameof(InMemory), "InMemory");
    }
}
