namespace Soap.UnitTests
{
    using Soap.Utility.Functions.Extensions;
    using Xunit;

    public class InheritsOrImplementsTests
    {
        [Fact]
        public void ShouldGetBaseTypesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(W)));

        [Fact]
        public void ShouldGetInterfacesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(J)));

        [Fact]
        public void ShouldGetInterfacesOfInterfacesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(K)));

        [Fact]
        public void ShouldGetItsBaseType() => Assert.True(new V().GetType().InheritsOrImplements(typeof(W)));

        [Fact]
        public void ShouldGetItself() => Assert.True(new V().GetType().InheritsOrImplements(typeof(V)));

        [Fact]
        public void ShouldGetObject() => Assert.True(new V().GetType().InheritsOrImplements(typeof(object)));

        [Fact]
        public void ShouldGetOpenGenericBaseTypesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(Z<>)));

        [Fact]
        public void ShouldGetOpenGenericInterfacesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(I<>)));

        [Fact]
        public void ShouldGetSpecificGenericBaseTypesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(Z<string>)));

        [Fact]
        public void ShouldGetSpecificGenericInterfacesDownTheChain() => Assert.True(new V().GetType().InheritsOrImplements(typeof(I<string>)));

        [Fact]
        public void ShouldNotGetUnimplementedTypes() => Assert.False(new V().GetType().InheritsOrImplements(typeof(INotImplemented)));
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
