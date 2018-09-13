namespace Soap.Tests
{
    using System;
    using System.Collections.Generic;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Xunit;

    public class ObjectExtensionsTests
    {
        public class WhenTestingWithFQNames
        {
            [Fact]
            public void ItShouldReturnTheRightValues()
            {
                Assert.Equal("string", typeof(string).AsTypeNameString(true));
                Assert.Equal("int[]", typeof(int[]).AsTypeNameString(true));
                Assert.Equal("int[][]", typeof(int[][]).AsTypeNameString(true));
                Assert.Equal("System.Collections.Generic.KeyValuePair<int, string>", typeof(KeyValuePair<int, string>).AsTypeNameString(true));
                Assert.Equal("System.Tuple<int, string>", typeof(Tuple<int, string>).AsTypeNameString(true));
                Assert.Equal("System.Tuple<KeyValuePair<object, long>, string>", typeof(Tuple<KeyValuePair<object, long>, string>).AsTypeNameString(true));
                Assert.Equal("System.Collections.Generic.List<Tuple<int, string>>", typeof(List<Tuple<int, string>>).AsTypeNameString(true));
                Assert.Equal("System.Tuple<short[], string>", typeof(Tuple<short[], string>).AsTypeNameString(true));
                Assert.Equal("Soap.Tests.ObjectExtensionsTests+TestNested", typeof(TestNested).AsTypeNameString(true));
                Assert.Equal("Soap.Tests.ObjectExtensionsTests+TestNestedGeneric<string>", typeof(TestNestedGeneric<string>).AsTypeNameString(true));
            }
        }

        public class WhenTestingWithShortNames
        {
            [Fact]
            public void ItShouldReturnTheRightValues()
            {
                Assert.Equal("string", typeof(string).AsTypeNameString());
                Assert.Equal("int[]", typeof(int[]).AsTypeNameString());
                Assert.Equal("int[][]", typeof(int[][]).AsTypeNameString());
                Assert.Equal("KeyValuePair<int, string>", typeof(KeyValuePair<int, string>).AsTypeNameString());
                Assert.Equal("Tuple<int, string>", typeof(Tuple<int, string>).AsTypeNameString());
                Assert.Equal("Tuple<KeyValuePair<object, long>, string>", typeof(Tuple<KeyValuePair<object, long>, string>).AsTypeNameString());
                Assert.Equal("List<Tuple<int, string>>", typeof(List<Tuple<int, string>>).AsTypeNameString());
                Assert.Equal("Tuple<short[], string>", typeof(Tuple<short[], string>).AsTypeNameString());
                Assert.Equal("ObjectExtensionsTests+TestNested", typeof(TestNested).AsTypeNameString());
                Assert.Equal("ObjectExtensionsTests+TestNestedGeneric<string>", typeof(TestNestedGeneric<string>).AsTypeNameString());
            }
        }

        internal class TestNested
        {
        }

        internal class TestNestedGeneric<T>
        {
        }
    }
}