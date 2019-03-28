namespace Soap.Pf.MessageContractsBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types
    /// </summary>
    public abstract class Enumeration : IComparable, IEquatable<Enumeration>, IComparer<Enumeration>
    {
        protected Enumeration()
        {
        }

        protected Enumeration(int key, string displayName)
        {
            Key = key;
            DisplayName = displayName;
        }

        public bool Active { get; set; }

        public string DisplayName { get; set; }

        public int Key { get; set; }

        public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
        {
            var absoluteDifference = Math.Abs(firstValue.Key - secondValue.Key);
            return absoluteDifference;
        }

        public static T FromDisplayName<T>(string displayName) where T : Enumeration, new()
        {
            var matchingItem = parse<T, string>(displayName, "display name", item => item.DisplayName == displayName);
            return matchingItem;
        }

        public static T FromKey<T>(int value) where T : Enumeration, new()
        {
            var matchingItem = parse<T, int>(value, "value", item => item.Key == value);
            return matchingItem;
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new T();
                var locatedValue = info.GetValue(instance) as T;

                if (locatedValue != null)
                {
                    yield return locatedValue;
                }
            }
        }

        public static bool operator ==(Enumeration left, Enumeration right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Enumeration left, Enumeration right)
        {
            return !Equals(left, right);
        }

        public int Compare(Enumeration x, Enumeration y)
        {
            return x.Key.CompareTo(y.Key);
        }

        public int CompareTo(object other)
        {
            return Key.CompareTo(((Enumeration)other).Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Enumeration)obj);
        }

        public bool Equals(Enumeration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key == other.Key;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Active.GetHashCode();
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Key;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        private static T parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration, new()
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);

            if (matchingItem == null)
            {
                var message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(T));
                throw new ApplicationException(message);
            }

            return matchingItem;
        }
    }
}