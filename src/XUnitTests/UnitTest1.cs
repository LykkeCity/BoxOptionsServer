using System;
using Xunit;

namespace XUnitTests
{
    public class UnitTest1
    {
        [Fact]        
        public void Test1()
        {
            Assert.Equal(4d, 2d + 2d);
        }
    }
}
