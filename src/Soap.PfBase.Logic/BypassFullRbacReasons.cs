
namespace Soap.PfBase.Logic
{
    using CircuitBoard;

    public class BypassFullRbacReasons : TypedEnumeration<BypassFullRbacReasons>
    {
        public static BypassFullRbacReasons InternalUseOnly = Create("internal-use", "Data being read is for use only within the service boundaries");

        /* Ideally for Reads there would be any easy way of knowing whether the data in the events you are sending
         back to the end user are from fields/aggregates which the user does/doesn't have access to but because of all the possible
         transformations along the way it seems basically impossible to know where the data has been sourced from when sending the event
         and equally impossible to know how it might be used at the callsite of the read method. So everything will be locked down by
         default, and you will have to specific a reason if you want to bypass security when rbac is enabled, it is not perfect
         because you can still then send this data to a user that shouldn't have it but it forces a developer to think at the callsite anyway.
         
         For writes you would ideally have a way of knowing if the aggregate your are changing, is, in the context of the current message,
         a security concern, but again there is not way of knowing. so we will have to let the developer decide */
    }
}