namespace Soap.UnitTests
{
    using System.Linq;
    using Newtonsoft.Json;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Objects.Binary;
    using Xunit;

    public class StatesSample : Enumeration<StatesSample>
    {
        public static StatesSample One = Create("1", nameof(StatesSample.One));

        public static StatesSample Three = Create("3", nameof(StatesSample.Three));

        public static StatesSample Two = Create("2", nameof(StatesSample.Two));
    }

    public class EnumerationFlagsTests
    {
        public class WhenWeAddASecondState
        {
            private readonly EnumerationFlags x;

            public WhenWeAddASecondState()
            {
                this.x = new EnumerationFlags(StatesSample.One);
                this.x.AddFlag(StatesSample.Two);
            }

            [Fact]
            public void ItShouldContainTheDefaultState() => Assert.True(this.x.HasFlag(StatesSample.One));

            [Fact]
            public void ItShouldContainTheNewState() => Assert.True(this.x.HasFlag(StatesSample.Two));

            [Fact]
            public void ItShouldNotContainAnyOtherStates() => Assert.Equal(2, this.x.Count());
        }

        public class WhenWeCreateAFlaggedState
        {
            private readonly EnumerationFlags x;

            public WhenWeCreateAFlaggedState()
            {
                this.x = new EnumerationFlags(StatesSample.One);
            }

            [Fact]
            public void ItShouldContainNoOtherStates() => Assert.Equal(1, this.x.Count());

            [Fact]
            public void ItShouldContainTheDefaultState() => Assert.True(this.x.HasFlag(StatesSample.One));
        }

        public class WhenWeDeserialize
        {
            [Fact]
            public void ItShouldWork()
            {
                var x = new EnumerationFlags(StatesSample.One);
                var json = x.ToJson(SerialiserIds.JsonDotNetDefault);
                var y = json.FromJson<EnumerationFlags>(SerialiserIds.JsonDotNetDefault);
                Assert.True(y.HasFlag(StatesSample.One));
            }
        }
        
        public class WhenWeConvertIntoFlagsAndBackToEnumerationItems
        {
            [Fact]
            public void ItShouldWork()
            {
                var x = new EnumerationFlags(StatesSample.One);
                x.AddFlag(StatesSample.Three);
                var json = x.ToJson(SerialiserIds.JsonDotNetDefault);
                var y = json.FromJson<EnumerationFlags>(SerialiserIds.JsonDotNetDefault);
                Assert.Equal(2, y.AsEnumerations<StatesSample>().Count);
                Assert.Equal(StatesSample.Three, y.AsEnumerations<StatesSample>().Last());
            }
        }

        public class WhenWeRemoveAllStates
        {
            [Fact]
            public void ItShouldNotThrowAnError()
            {
                var x = new EnumerationFlags(StatesSample.One);
                x.RemoveFlag(StatesSample.One);
            }
        }

        public class WhenWeRemoveAState
        {
            private readonly EnumerationFlags x;

            public WhenWeRemoveAState()
            {
                this.x = new EnumerationFlags(StatesSample.One);
                this.x.AddFlag(StatesSample.Two);
                this.x.RemoveFlag(StatesSample.One);
            }

            [Fact]
            public void ItShouldNotRemoveAnyOtherStates() => Assert.Equal(1, this.x.Count());

            [Fact]
            public void ItShouldRemoveTheState() => Assert.True(!this.x.HasFlag(StatesSample.One));
        }
    }
}