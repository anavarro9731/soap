namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using CircuitBoard;
    using Soap.Interfaces.Messages;

    public static class ApiMessageExt
    {

        public static ApiMessage ToApiMessage(this MessageFailedAllRetries message)
        {
            return message.Map(
                x => message.SerialisedMessage.FromJson<ApiMessage>(
                    SerialiserIds.GetInstanceFromKey(message.SerialiserId),
                    message.TypeName));
        }
        
                public static void RequiredNotNullOrThrow(this ApiMessage message)
        {
            var errors = new StringBuilder();
            AccumulateErrors(errors, message.GetType(), message);
            if (errors.Length > 0)
            {
                throw new CircuitException("The message has required properties that are null" + Environment.NewLine + errors);
            }

            static void AccumulateErrors(StringBuilder errors, Type aType, object o)
            {
                //*FRAGILE* based on expected types
                foreach (var propertyInfo in aType.GetProperties())
                {
                    var propertyValue = propertyInfo.GetValue(o);
                    var propertyType = propertyInfo.PropertyType;

                    if (propertyType.IsSystemType())
                    {
                        GuardAgainstRequiredAndNull(errors, propertyInfo, propertyValue);

                        if (propertyValue != null && propertyType.InheritsOrImplements(typeof(List<>)))
                        {
                            var list = propertyValue.CastOrError<IList>();

                            if (list.Count > 0 && list[0].GetType().IsSystemType() == false)
                            {
                                
                                var elementProperties = list[0].GetType().GetProperties();
                                foreach (var item in list)
                                {
                                    foreach (var elementPropertyInfo in elementProperties)
                                        GuardAgainstRequiredAndNull(errors, elementPropertyInfo, elementPropertyInfo.GetValue(item));
                                }
                            }
                        }
                    }
                    else
                    {
                        GuardAgainstRequiredAndNull(errors, propertyInfo, propertyValue);
                        if (propertyValue != null && propertyType != typeof(MessageHeaders))
                        {
                            AccumulateErrors(errors, propertyType, propertyValue);
                        }
                    }
                }
            }

            static void GuardAgainstRequiredAndNull(StringBuilder errors, PropertyInfo prop, object value)
            {
                //* This only works because boxing the underlying Nullable<T> as an object, loses the nullability and you get null or a value
                if (prop.HasAttribute(typeof(RequiredAttribute)) && value == null)
                {
                    errors.AppendLine(
                        $"The property {prop.Name} in {prop.DeclaringType.FullName} is marked with the {nameof(RequiredAttribute)} but the value is null");
                }
            }
        }
    }
}
