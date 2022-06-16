namespace Soap.Utility.Objects.Binary
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard;
    using Soap.Utility.Functions.Operations;

    /* stores a bunch of different flags in a single serialisable class
     avoids lots of bool properties and can also be used to represent 
     a current state in a state-machine
     
     Do not use typeof(T).IsEnum when checking types, use incoming instance of T
     sometimes returns false with T = System.Enum

     */
    public static class FlagsExt
    {
        private static void ThrowIf(bool condition, string message)
        {
            if (condition) throw new CircuitException(message);
        }
        
        public static EnumFlags AddFlag<T>(this EnumFlags flags, T newState)
        {
            
          ThrowIf(!newState.GetType().IsEnum, "T must be an enumerated type");
          ThrowIf(flags.Values.Contains(Convert.ToInt32(newState)), "State already flagged");
          ThrowIf(Convert.ToInt32(newState) == 0, "Enums with 0 valued items are not allowed as zero is the defined default for an enum and should not be used for values other than unknown");
            
            flags.Values.Add(Convert.ToInt32(newState));

            return flags;
        }

        public static bool HasFlag(this EnumFlags flags, int item) => flags.Values.Contains(item);
        public static bool HasFlag<T>(this EnumFlags flags, T state) => flags.Values.Contains(Convert.ToInt32(state));
        
        public static int Count(this EnumFlags flags) => flags.Values.Count;
        
        public static EnumFlags RemoveFlag<T>(this EnumFlags flags, T state)
        {
         ThrowIf(!state.GetType().IsEnum, "T must be an enumerated type");
         ThrowIf(flags.Values.Count == 1 && flags.Values.Contains(Convert.ToInt32(state)), "Cannot remove last state");

            flags.Values.Remove(Convert.ToInt32(state));

            return flags;
        }

        public static void ReplaceFlag<T>(this EnumFlags flags, T old, T @new)
        {
            flags.RemoveFlag(old);
            flags.AddFlag(@new);
        }

        public static IReadOnlyList<T> AsTypedEnum<T>(this EnumFlags flags)
        {
            return flags.Values.ConvertAll(i => (T)Enum.Parse(typeof(T), i.ToString()));
        }
    }

    //- doesn't inherit from List<int> to ensure easy serialisation back to Flags
    public class EnumFlags
    {
        public EnumFlags(Enum initialState)
        {
            this.AddFlag(initialState);
        }

        public EnumFlags()
        {
        }

        public List<int> Values { get; set; } = new List<int>();
    }
}
