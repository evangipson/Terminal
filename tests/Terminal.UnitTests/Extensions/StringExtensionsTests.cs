using Terminal.Extensions;

namespace Terminal.UnitTests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void Repeat_ShouldNotRepeatString_WithNull()
        {
            string? stringToRepeat = null;

            var result = stringToRepeat.Repeat(10);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(int.MinValue)]
        public void Repeat_ShouldNotRepeatString_WithInvalidCount(int invalidRepeatCount)
        {
            var stringToRepeat = "*";

            var result = stringToRepeat.Repeat(invalidRepeatCount);

            Assert.Equal(stringToRepeat, result);
        }

        [Fact]
        public void Repeat_ShouldRepeatString_WithValidString()
        {
            var expected = 10;
            var stringToRepeat = "*";

            var result = stringToRepeat.Repeat(expected);

            Assert.Equal(expected, result.Length);
        }
    }
}
