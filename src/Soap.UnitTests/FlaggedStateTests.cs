namespace Soap.UnitTests
{
    using System;
    using Newtonsoft.Json;
    using Soap.Utility.Models;
    using Soap.Utility.Objects.Binary;
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
            private readonly Flags x;

            public WhenWeAddASecondState()
            {
                this.x = new Flags(StatesSample.State1);
                this.x.AddState(StatesSample.State2);
            }

            [Fact]
            public void ItShouldContainTheDefaultState() => Assert.True(this.x.Contains(Convert.ToInt32(StatesSample.State1)));

            [Fact]
            public void ItShouldContainTheNewState() => Assert.True(this.x.Contains(Convert.ToInt32(StatesSample.State2)));

            [Fact]
            public void ItShouldNotContainAnyOtherStates() => Assert.Equal(2, this.x.Count());
        }

        public class WhenWeCreateAFlaggedState
        {
            private readonly Flags x;

            public WhenWeCreateAFlaggedState()
            {
                this.x = new Flags(StatesSample.State1);
            }

            [Fact]
            public void ItShouldContainNoOtherStates() => Assert.Equal(1, this.x.Count());

            [Fact]
            public void ItShouldContainTheDefaultState() => Assert.True(this.x.Contains(Convert.ToInt32(StatesSample.State1)));
        }

        public class WhenWeDeserialize
        {
            [Fact]
            public void ItShouldWork()
            {
                var x = new Flags(StatesSample.State1);
                var json = JsonConvert.SerializeObject(x);
                var y = JsonConvert.DeserializeObject<Flags>(json);
                Assert.True(y.HasState(StatesSample.State1));
            }
        }

        public class WhenWeRemoveAState
        {
            private readonly Flags x;

            public WhenWeRemoveAState()
            {
                this.x = new Flags(StatesSample.State1);
                this.x.AddState(StatesSample.State2);
                this.x.RemoveState(StatesSample.State1);
            }

            [Fact]
            public void ItShouldNotRemoveAnyOtherStates() => Assert.Equal(1, this.x.Count());

            [Fact]
            public void ItShouldRemoveTheState() => Assert.True(!this.x.Contains(Convert.ToInt32(StatesSample.State1)));
        }

        public class WhenWeRemoveTheDefaultState
        {
            [Fact]
            public void ItShouldThrowAnError()
            {
                Assert.Throws<DomainException>(
                    () =>
                        {
                        var x = new Flags(StatesSample.State1);
                        x.RemoveState(StatesSample.State1);
                        });
            }
        }
    }
}