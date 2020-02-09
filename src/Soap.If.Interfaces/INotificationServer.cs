namespace Soap.Interfaces
{
    public interface INotificationServer
    {
        void Notify(string text, string subject, string[] sendTo);
    }
}