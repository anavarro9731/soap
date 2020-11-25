namespace Soap.UnitTests
{
    using Soap.Utility.Functions.Extensions;
    using Xunit;

    public class InheritsOrImplementsTests
    {
        [Fact]
        public void ShouldGetBaseTypesDownTheChain() => Assert.True(new V().Is(typeof(W)));

        [Fact]
        public void ShouldGetInterfacesDownTheChain() => Assert.True(new V().Is(typeof(J)));

        [Fact]
        public void ShouldGetInterfacesOfInterfacesDownTheChain() => Assert.True(new V().Is(typeof(K)));

        [Fact]
        public void ShouldGetItsBaseType() => Assert.True(new V().Is(typeof(W)));

        [Fact]
        public void ShouldGetItself() => Assert.True(new V().Is(typeof(V)));

        [Fact]
        public void ShouldGetObject() => Assert.True(new V().Is(typeof(object)));

        [Fact]
        public void ShouldGetOpenGenericBaseTypesDownTheChain() => Assert.True(new V().Is(typeof(Z<>)));

        [Fact]
        public void ShouldGetOpenGenericInterfacesDownTheChain() => Assert.True(new V().Is(typeof(I<>)));

        [Fact]
        public void ShouldGetSpecificGenericBaseTypesDownTheChain() => Assert.True(new V().Is(typeof(Z<string>)));

        [Fact]
        public void ShouldGetSpecificGenericInterfacesDownTheChain() => Assert.True(new V().Is(typeof(I<string>)));

        [Fact]
        public void ShouldNotGetUnimplementedTypes() => Assert.False(new V().Is(typeof(INotImplemented)));
    }

    internal class V : W
    {
    }

    internal class W : X<string>
    {
    }

    internal class X<T> : Y<T>
    {
    }

    internal class Y<T> : Z<T>, I<string>, J
    {
    }

    internal class Z<T>
    {
    }

    internal interface I<T> : K
    {
    }

    internal interface J
    {
    }

    internal interface K
    {
    }

    internal interface INotImplemented
    {
    }
}
