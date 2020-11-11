namespace Soap.Interfaces.Messages
{
    using System;
    using System.Collections.Generic;

    /* stores a bunch of different flags in a single serialisable class
     avoids lots of bool properties and can also be used to represent 
     a current state in a state-machine
     
     */
    public static class EnumerationExt
    {
        public static EnumerationFlags AddFlag<T>(this EnumerationFlags flags, T value) where T : Enumeration
        {
            if (flags.Keys.Contains(value.Key)) throw new ArgumentException("State already flagged");

            flags.Keys.Add(value.Key);

            return flags;
        }

        public static IReadOnlyList<T> AsEnumerations<T>(this EnumerationFlags flags) where T : Enumeration, new() =>
            flags.Keys.ConvertAll(Enumeration.FromKey<T>);

        public static int Count(this EnumerationFlags flags) => flags.Keys.Count;

        public static bool HasFlag(this EnumerationFlags flags, Enumeration item) => flags.Keys.Contains(item.Key);

        public static bool HasFlag(this EnumerationFlags flags, string key) => flags.Keys.Exists(x => x == key);

        public static EnumerationFlags RemoveFlag<T>(this EnumerationFlags flags, T value) where T : Enumeration
        {
            flags.Keys.Remove(value.Key);

            return flags;
        }

        public static void ReplaceFlag(this EnumerationFlags flags, Enumeration old, Enumeration @new)
        {
            flags.RemoveFlag(old);
            flags.AddFlag(@new);
        }
    }

    //- doesn't inherit from List<string> to ensure reliable serialisation back to EnumerationFlags
    public class EnumerationFlags
    {
        public EnumerationFlags(Enumeration initialState)
        {
            this.AddFlag(initialState);
        }

        public EnumerationFlags()
        {
        }

        public List<string> Keys { get; set; } = new List<string>();
    }
}