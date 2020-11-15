namespace Soap.UnitTests
{
    using System;
    using Newtonsoft.Json;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Objects.Binary;
    using Xunit;

    public class EnumFlagTests
    {
        public enum EnumFlagTestStates
        {
            State1 = 1,

            State2 = 2,

            State3 = 3
        }

        public class WhenWeAddASecondState
        {
            private readonly EnumFlags x;

            public WhenWeAddASecondState()
            {
                this.x = new EnumFlags(EnumFlagTestStates.State1);
                this.x.AddFlag(EnumFlagTestStates.State2);
            }

            [Fact]
            public void ItShouldContainTheDefaultState() => Assert.True(this.x.HasFlag(Convert.ToInt32(EnumFlagTestStates.State1)));

            [Fact]
            public void ItShouldContainTheNewState() => Assert.True(this.x.HasFlag(Convert.ToInt32(EnumFlagTestStates.State2)));

            [Fact]
            public void ItShouldNotContainAnyOtherStates() => Assert.Equal(2, this.x.Count());
        }

        public class WhenWeCreateAFlaggedState
        {
            private readonly EnumFlags x;

            public WhenWeCreateAFlaggedState()
            {
                this.x = new EnumFlags(EnumFlagTestStates.State1);
            }

            [Fact]
            public void ItShouldContainNoOtherStates() => Assert.Equal(1, this.x.Count());

            [Fact]
            public void ItShouldContainTheDefaultState() => Assert.True(this.x.HasFlag(Convert.ToInt32(EnumFlagTestStates.State1)));
        }

        public class WhenWeDeserialize
        {
            [Fact]
            public void ItShouldWork()
            {
                var x = new EnumFlags(EnumFlagTestStates.State1);
                var json = x.ToJson(SerialiserIds.JsonDotNetDefault);
                var y = json.FromJson<EnumFlags>(SerialiserIds.JsonDotNetDefault);
                Assert.True(y.HasFlag(EnumFlagTestStates.State1));
            }
        }

        public class WhenWeRemoveAState
        {
            private readonly EnumFlags x;

            public WhenWeRemoveAState()
            {
                this.x = new EnumFlags(EnumFlagTestStates.State1);
                this.x.AddFlag(EnumFlagTestStates.State2);
                this.x.RemoveFlag(EnumFlagTestStates.State1);
            }

            [Fact]
            public void ItShouldNotRemoveAnyOtherStates() => Assert.Equal(1, this.x.Count());

            [Fact]
            public void ItShouldRemoveTheState() => Assert.True(!this.x.HasFlag(Convert.ToInt32(EnumFlagTestStates.State1)));
        }

        public class WhenWeRemoveTheDefaultState
        {
            [Fact]
            public void ItShouldThrowAnError()
            {
                Assert.ThrowsAny<Exception>(
                    () =>
                        {
                        var x = new EnumFlags(EnumFlagTestStates.State1);
                        x.RemoveFlag(EnumFlagTestStates.State1);
                        });
            }
        }
    }
}