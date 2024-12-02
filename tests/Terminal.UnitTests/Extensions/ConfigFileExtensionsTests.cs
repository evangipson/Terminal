using Terminal.Extensions;

namespace Terminal.UnitTests.Extensions
{
    public class ConfigFileExtensionsTests
    {
        [Fact]
        public void GetLatestIntegerConfig_ShouldReturnInteger_WithValidKey()
        {
            var expected = 15;
            var testKey = "key";
            Dictionary<string, string> testConfig = new()
            {
                [testKey] = $"{expected}"
            };

            var result = testConfig.GetLatestIntegerConfig(testKey);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetLatestIntegerConfig_ShouldReturnDefaultValue_WithInvalidValue()
        {
            var expectedDefault = 200;
            var testKey = "key";
            Dictionary<string, string> testConfig = new()
            {
                [testKey] = "not-a-number"
            };

            var result = testConfig.GetLatestIntegerConfig(testKey, expectedDefault);

            Assert.Equal(expectedDefault, result);
        }

        [Fact]
        public void GetLatestIntegerConfig_ShouldReturnDefaultValue_UsingNullConfig()
        {
            var expectedDefault = 200;
            Dictionary<string, string>? testConfig = null;

            var result = testConfig.GetLatestIntegerConfig("some-key", expectedDefault);

            Assert.Equal(expectedDefault, result);
        }
    }
}
