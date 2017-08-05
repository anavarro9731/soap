namespace Palmtree.ApiPlatform.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Palmtree.ApiPlatform.Utility.PureFunctions;

    public class FlaggedState
    {
        [JsonProperty]
        private readonly List<int> states;

        [JsonProperty]
        private readonly string typeName;

        private FlaggedState(int initialState, string typeName)
        {
            this.states = new List<int>
            {
                initialState
            };
            this.typeName = typeName;
        }

        [JsonConstructor]
        private FlaggedState(List<int> states, string typeName)
        {
            this.states = states;
            this.typeName = typeName;
        }

        public int Count => this.states.Count;

        public int this[int index] => this.states[index];

        public static FlaggedState Create<T>(T initialState) where T : IConvertible
        {
            //do not use typeof(T).IsEnum, sometimes returns false with T = System.Enum
            Guard.Against(!initialState.GetType().IsEnum, "T must be an enumerated type");

            return new FlaggedState(Convert.ToInt32(initialState), initialState.GetType().FullName);
        }

        public static FlaggedState Create<T>()
        {
            return new FlaggedState(0, typeof(T).FullName);
        }

        public void AddState<T>(T state) where T : IConvertible
        {
            //do not use typeof(T).IsEnum, sometimes returns false with T = System.Enum
            Guard.Against(!state.GetType().IsEnum, "T must be an enumerated type");
            Guard.Against(this.typeName != state.GetType().FullName, "T must be of type " + this.typeName);
            Guard.Against(this.states.Contains(Convert.ToInt32(state)), "State already flagged");

            this.states.Add(Convert.ToInt32(state));
        }

        public bool Contains(int item)
        {
            return this.states.Contains(item);
        }

        public bool HasState<T>(T state) where T : IConvertible
        {
            return StatesAs<T>().Contains(state);
        }

        public void RemoveState<T>(T state) where T : IConvertible
        {
            //do not use typeof(T).IsEnum, sometimes returns false with T = System.Enum
            Guard.Against(!state.GetType().IsEnum, "T must be an enumerated type");
            Guard.Against(this.typeName != state.GetType().FullName, "T must be of type " + this.typeName);
            Guard.Against(this.states.Count == 1 && this.states.Contains(Convert.ToInt32(state)), "Cannot remove last state");

            this.states.Remove(Convert.ToInt32(state));
        }

        public void ReplaceState<T>(T old, T @new) where T : IConvertible
        {
            RemoveState(old);
            AddState(@new);
        }

        public IReadOnlyList<T> StatesAs<T>() where T : IConvertible
        {
            return this.states.ConvertAll(i => (T)Enum.Parse(typeof(T), i.ToString()));
        }
    }
}
