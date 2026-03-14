using System.IO;
using psi25_project.Utils;
using Xunit;

namespace Geohunt.Tests.Utils
{
    public class AddressProviderTests
    {
        [Fact]
        public void GetRandomAddress_ReturnsOneOfTheAddresses()
        {
            // Arrange
            string[] addresses = { "Address1", "Address2", "Address3" };
            string tempFile = Path.GetTempFileName();
            File.WriteAllLines(tempFile, addresses);

            // Act
            string selected = AddressProvider.GetRandomAddress(tempFile);

            // Assert
            Assert.Contains(selected, addresses);

            // Cleanup
            File.Delete(tempFile);
        }

        [Fact]
        public void GetRandomAddress_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            // Arrange
            string nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.txt");

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => AddressProvider.GetRandomAddress(nonExistentFile));
            Assert.Contains("Address file not found", ex.Message);
        }

        [Fact]
        public void GetRandomAddress_ThrowsException_WhenFileIsEmpty()
        {
            // Arrange
            string tempFile = Path.GetTempFileName(); // empty file

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => AddressProvider.GetRandomAddress(tempFile));
            Assert.Equal("No valid address found.", ex.Message);

            File.Delete(tempFile);
        }
    }
}
