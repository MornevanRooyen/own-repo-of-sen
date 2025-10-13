namespace Sen381.Tests
{
    public class SetupTest
    {
        [Fact]
        public void Test1()
        {
            int expected = 2;
            int actual = 1 + 1;

            Assert.Equal(expected, actual);
        }
    }
}