namespace Soap.Utility.Functions.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;

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
        public static TValue GetAttributeValue<TAttribute, TValue>(this Type type, Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            if (type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() is TAttribute att) return valueSelector(att);
            return default;
        }
        
        public static bool HasAttribute<TAttribute>(this Type typeWithAttribute) where TAttribute : Attribute {
            return typeWithAttribute.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() is TAttribute;
        }
        
        public static bool HasAttribute(this PropertyInfo prop, Type attributeType){
            var att = prop.GetCustomAttributes(attributeType, true).FirstOrDefault();
            return att != null;
        }
    }
}
