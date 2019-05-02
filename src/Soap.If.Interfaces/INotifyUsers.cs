namespace Soap.If.Interfaces
{
    public interface INotifyUsers
    {
        void Notify(string text, string subject, string[] sendTo);
    }
}