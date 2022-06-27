namespace Soap.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Xunit;
    using Xunit.Abstractions;

    public class Play
    {
        private readonly ITestOutputHelper o;

        public Play(ITestOutputHelper o)
        {
            this.o = o;
        }


        [Fact]
        public void Q()
        {
            
        }

        [Fact]
        public void P()
        {

            var two = new Two("-", "-")
            {
                Description = "Two"
            };

            var one = new One("-", "-")
            {
                Title = "One"
            };
            
            Assert.True(two.Equals(one));
        }
        
        public class Two : Enumeration
        {
            public Two(string key, string value)
                : base(key, value)
            {
            }

            public string Description;

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
        }

        public class One : Enumeration
        {
            public One(string key, string value)
                : base(key, value)
            {
            }

            public string Title;
        }


        
    }
}