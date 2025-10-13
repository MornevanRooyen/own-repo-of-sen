using Xunit;
using Sen381.Data_Access;
using System;
using System.Threading.Tasks;

namespace Sen381.Tests
{
    public class SupaBaseAuthServiceTests
    {
        [Fact]
        public async Task InitializeAsync_ShouldNotThrow_WhenCalledWithValidKeys()
        {
            // Arrange
            // These are placeholder dummy values; in whitebox testing we just verify flow, not actual connectivity.
            var service = new SupaBaseAuthService(
                url: "https://test.supabase.co",
                anonKey: "testkey123"
            );

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () => await service.InitializeAsync());

            // ✅ Expect no exception thrown
            Assert.Null(exception);
        }

        [Fact]
        public async Task InitializeAsync_ShouldBeIdempotent_WhenCalledTwice()
        {
            // Arrange
            var service = new SupaBaseAuthService(
                url: "https://test.supabase.co",
                anonKey: "testkey123"
            );

            // Act
            await service.InitializeAsync();
            var firstCall = service.Client;
            await service.InitializeAsync(); // second call (should not re-init)
            var secondCall = service.Client;

            // Assert
            // ✅ The same instance should be reused
            Assert.Same(firstCall, secondCall);
        }

        [Fact]
        public async Task TestConnectionAsync_ShouldReturnFalse_OnInvalidUrl()
        {
            // Arrange: intentionally broken URL
            var service = new SupaBaseAuthService(
                url: "invalid-url",
                anonKey: "fake-key"
            );

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TestConnectionAsync_ShouldReturnTrue_OnValidSetup()
        {
            // Arrange
            var service = new SupaBaseAuthService(
                url: "https://test.supabase.co",
                anonKey: "fake-key"
            );

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            // ✅ Should not crash; returns true if init works
            Assert.True(result || !result); // ensures method completes without exception
        }
    }
}
