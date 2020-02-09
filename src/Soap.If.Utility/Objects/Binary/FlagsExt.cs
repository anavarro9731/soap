namespace Soap.Utility.Objects.Binary
{
    using System;
    using System.Collections.Generic;
    using Soap.Utility.Functions.Operations;

    /* stores a bunch of different flags in a single serialisable class
     avoids lots of bool properties and can also be used to represent 
     a current state in a state-machine
     
     Do not use typeof(T).IsEnum when checking types, use incoming instance of T
     sometimes returns false with T = System.Enum

     */
    public static class FlagsExt
    {
        public static Flags AddState<T>(this Flags flags, T newState)
        {
            Guard.Against(!newState.GetType().IsEnum, "T must be an enumerated type");
            Guard.Against(flags.Values.Contains(Convert.ToInt32(newState)), "State already flagged");

            flags.Values.Remove(0);
            flags.Values.Add(Convert.ToInt32(newState));

            return flags;
        }

        public static bool Contains(this Flags flags, int item)
        {
            return flags.Values.Contains(item);
        }

        public static int Count(this Flags flags)
        {
            return flags.Values.Count;
        }

        public static bool HasState<T>(this Flags flags, T state)
        {
            return flags.Values.Contains(Convert.ToInt32(state));
        }

        public static Flags RemoveState<T>(this Flags flags, T state)
        {
            Guard.Against(!state.GetType().IsEnum, "T must be an enumerated type");
            Guard.Against(flags.Values.Count == 1 && flags.Values.Contains(Convert.ToInt32(state)), "Cannot remove last state");

            flags.Values.Remove(Convert.ToInt32(state));

            return flags;
        }

        public static void ReplaceState<T>(this Flags flags, T old, T @new)
        {
            flags.RemoveState(old);
            flags.AddState(@new);
        }

        public static IReadOnlyList<T> StatesAsT<T>(this Flags flags)
        {
            return flags.Values.ConvertAll(i => (T)Enum.Parse(typeof(T), i.ToString()));
        }
    }

    //- doesn't inherit from List<int> to ensure easy serialisation back to Flags
    public class Flags 
    {
        public Flags(Enum initialState)
        {
            FlagsExt.AddState(this, initialState);
        }

        public List<int> Values = new List<int>();
    }
}