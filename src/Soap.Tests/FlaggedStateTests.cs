namespace Soap.Tests
{
    using System;
    using Newtonsoft.Json;
    using Soap.Utility;
    using Soap.Utility.PureFunctions;
    using Xunit;

    public class FlaggedStateTests
    {
        public enum StatesSample
        {
            State1 = 1,

            State2 = 2,

            State3 = 3
        }

        public class WhenWeAddASecondState
        {
            private readonly FlaggedState x;

            public WhenWeAddASecondState()
            {
                this.x = FlaggedState.Create(StatesSample.State1);
                this.x.AddState(StatesSample.State2);
            }

            [Fact]
            public void ItShouldContainTheDefaultState()
            {
                Assert.True(this.x.Contains(Convert.ToInt32(StatesSample.State1)));
            }

            [Fact]
            public void ItShouldContainTheNewState()
            {
                Assert.True(this.x.Contains(Convert.ToInt32(StatesSample.State2)));
            }

            [Fact]
            public void ItShouldNotContainAnyOtherStates()
            {
                Assert.Equal(2, this.x.Count);
            }
        }

        public class WhenWeCreateAFlaggedState
        {
            private readonly FlaggedState x;

            public WhenWeCreateAFlaggedState()
            {
                this.x = FlaggedState.Create(StatesSample.State1);
            }

            [Fact]
            public void ItShouldContainNoOtherStates()
            {
                Assert.Equal(1, this.x.Count);
            }

            [Fact]
            public void ItShouldContainTheDefaultState()
            {
                Assert.True(this.x.Contains(Convert.ToInt32(StatesSample.State1)));
            }
        }

        public class WhenWeDeserialize
        {
            [Fact]
            public void ItShouldWork()
            {
                var x = FlaggedState.Create(StatesSample.State1);
                var json = JsonConvert.SerializeObject(x);
                var y = JsonConvert.DeserializeObject<FlaggedState>(json);
                Assert.True(y.HasState(StatesSample.State1));
            }
        }

        public class WhenWeRemoveAState
        {
            private readonly FlaggedState x;

            public WhenWeRemoveAState()
            {
                this.x = FlaggedState.Create(StatesSample.State1);
                this.x.AddState(StatesSample.State2);
                this.x.RemoveState(StatesSample.State1);
            }

            [Fact]
            public void ItShouldNotRemoveAnyOtherStates()
            {
                Assert.Equal(1, this.x.Count);
            }

            [Fact]
            public void ItShouldRemoveTheState()
            {
                Assert.True(!this.x.Contains(Convert.ToInt32(StatesSample.State1)));
            }
        }

        public class WhenWeRemoveTheDefaultState
        {
            [Fact]
            public void ItShouldThrowAnError()
            {
                Assert.Throws<DomainException>(
                    () =>
                        {
                        var x = FlaggedState.Create(StatesSample.State1);
                        x.RemoveState(StatesSample.State1);
                        });
            }
        }
    }
}
