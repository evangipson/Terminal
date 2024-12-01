using Terminal.Constants;
using Terminal.Extensions;
using Terminal.Models;

namespace Terminal.UnitTests.Extensions
{
    public class DirectoryExtensionsTests
    {
        private readonly List<DirectoryEntity> _defaultDirectory;

        public DirectoryExtensionsTests()
        {
            _defaultDirectory = DirectoryConstants.GetDefaultDirectoryStructure();
        }

        [Fact]
        public void FindEntity_ShouldReturnNull_WithNullEntityName()
        {
            var result = Root.FindEntity(null);

            Assert.Null(result);
        }

        [Fact]
        public void FindEntity_ShouldReturnExpectedEntity_WithValidEntityName()
        {
            var expected = "ethernet";

            var result = Root.FindEntity(expected);

            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public void FindEntity_ShouldReturnNull_WithInvalidEntityName()
        {
            var expected = "entity-that-shall-not-be-named";

            var result = Root.FindEntity(expected);

            Assert.Null(result);
        }

        [Fact]
        public void FindEntity_ShouldReturnItself_WithMatchingEntityName()
        {
            var expected = TerminalCharactersConstants.Separator.ToString();

            var result = Root.FindEntity(expected);

            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public void FindFile_ShouldReturnNull_WithNullFileName()
        {
            var result = Root.FindFile(null);

            Assert.Null(result);
        }

        [Fact]
        public void FindFile_ShouldReturnExpectedFile_WithValidFileName()
        {
            var expected = "ethernet";

            var result = Root.FindFile(expected);

            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public void FindFile_ShouldReturnNull_WithInvalidFileName()
        {
            var expected = "file-that-shall-not-be-named";

            var result = Root.FindFile(expected);

            Assert.Null(result);
        }

        [Fact]
        public void FindDirectory_ShouldReturnNull_WithNullDirectoryName()
        {
            var result = Root.FindDirectory(null);

            Assert.Null(result);
        }

        [Fact]
        public void FindDirectory_ShouldReturnExpectedDirectory_WithValidDirectoryName()
        {
            var expected = "device";

            var result = Root.FindDirectory(expected);

            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public void FindDirectory_ShouldReturnExpectedDirectory_WithAbsolutePath()
        {
            var expected = "mail";
            var absolutePath = $"/users/user/home/{expected}";

            var result = Root.FindDirectory(absolutePath);

            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public void FindDirectory_ShouldReturnNull_WithInvalidDirectoryName()
        {
            var expected = "directory-that-shall-not-be-named";

            var result = Root.FindDirectory(expected);

            Assert.Null(result);
        }

        [Fact]
        public void FindDirectory_ShouldReturnNull_WithInvalidId()
        {
            var expected = "mail";
            var directoryId = Root.FindDirectory(expected).Id;

            var result = Root.FindDirectory(directoryId);

            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public void FindDirectory_ShouldReturnItself_WithOwnId()
        {
            var result = Root.FindDirectory(Root.Id);

            Assert.Equal(Root, result);
        }

        private DirectoryEntity Root => _defaultDirectory.First(x => x.IsRoot);
    }
}
