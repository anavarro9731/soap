namespace Soap.NotificationServer
{
    using CircuitBoard;

    public class NotificationChannelTypes : TypedEnumeration<NotificationChannelTypes>
    {
        public static NotificationChannelTypes Email = Create(nameof(Email), "Email");
    }
}
