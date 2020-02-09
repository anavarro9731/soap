namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Linq;

    public static class AttributeExt
    {
        //from any enum
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name) // I prefer to get attributes this way
                       .GetCustomAttributes(false)
                       .OfType<TAttribute>()
                       .SingleOrDefault();
        }

        //from any class or struct
        public static TValue GetAttribute<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null) return valueSelector(att);
            return default(TValue);
        }
    }
}
