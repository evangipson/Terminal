using Terminal.Extensions;

namespace Terminal.UnitTests.Extensions
{
    public class NumberExtensionsTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(int.MaxValue, 0, 0, 0)]
        [InlineData(int.MaxValue, 0, int.MaxValue, 0)]
        [InlineData(0, int.MaxValue, int.MaxValue, 0)]
        [InlineData(0, 0, int.MaxValue, int.MaxValue)]
        public void ConvertRange_ShouldNotConvert_WithInvalidRange(int originalMin, int originalMax, int newMin, int newMax)
        {
            var numberToConvert = 50;

            var result = numberToConvert.ConvertRange(originalMin, originalMax, newMin, newMax);

            Assert.Equal(numberToConvert, result);
        }

        [Fact]
        public void ConvertRange_ShouldConvert_WithValidRange()
        {
            var numberToConvert = 50;

            var result = numberToConvert.ConvertRange(0, 100, 0, 200);

            Assert.Equal(numberToConvert * 2, result);
        }

        [Fact]
        public void ConvertRange_ShouldConvert_WithFloatRange()
        {
            var numberToConvert = 50;

            var result = numberToConvert.ConvertRange(0, 100, 0f, 200f);

            Assert.Equal(numberToConvert * 2, result);
        }
    }
}
