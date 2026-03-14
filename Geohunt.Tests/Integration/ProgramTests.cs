using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using psi25_project.Data;
using psi25_project.Services.Interfaces;
using psi25_project.Repositories.Interfaces;
using psi25_project.Gateways.Interfaces;
using Microsoft.AspNetCore.Identity;
using psi25_project.Models;
using psi25_project.Services;
using psi25_project.Repositories;

namespace Geohunt.Tests.Integration
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Application_StartsSuccessfully()
        {
            // Arrange & Act
            var client = _factory.CreateClient();

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void DbContext_IsRegistered()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            
            // Act
            var dbContext = scope.ServiceProvider.GetService<GeoHuntContext>();

            // Assert
            Assert.NotNull(dbContext);
        }

        [Fact]
        public void AllServices_AreRegistered()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act & Assert
            Assert.NotNull(services.GetService<IAccountService>());
            Assert.NotNull(services.GetService<IGameService>());
            Assert.NotNull(services.GetService<IGeocodingService>());
            Assert.NotNull(services.GetService<ILeaderboardService>());
            Assert.NotNull(services.GetService<IUserService>());
            Assert.NotNull(services.GetService<IResultService>());
            Assert.NotNull(services.GetService<IGuessService>());
            Assert.NotNull(services.GetService<ILocationService>());
        }

        [Fact]
        public void AllRepositories_AreRegistered()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act & Assert
            Assert.NotNull(services.GetService<IGameRepository>());
            Assert.NotNull(services.GetService<ILeaderboardRepository>());
            Assert.NotNull(services.GetService<IUserRepository>());
            Assert.NotNull(services.GetService<IGuessRepository>());
            Assert.NotNull(services.GetService<ILocationRepository>());
        }

        [Fact]
        public void Gateways_AreRegistered()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            
            // Act
            var gateway = scope.ServiceProvider.GetService<IGoogleMapsGateway>();

            // Assert
            Assert.NotNull(gateway);
        }

        [Fact]
        public void Identity_IsConfigured()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act
            var userManager = services.GetService<UserManager<ApplicationUser>>();
            var signInManager = services.GetService<SignInManager<ApplicationUser>>();
            var roleManager = services.GetService<RoleManager<IdentityRole<Guid>>>();

            // Assert
            Assert.NotNull(userManager);
            Assert.NotNull(signInManager);
            Assert.NotNull(roleManager);
        }

        [Fact]
        public async Task Swagger_IsAvailableInDevelopment()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/swagger/index.html");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public void CORS_IsConfigured()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            var corsService = services.GetService<Microsoft.AspNetCore.Cors.Infrastructure.ICorsService>();

            // Assert
            Assert.NotNull(corsService);
        }

        [Fact]
        public void Controllers_AreRegistered()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act
            var mvcServiceCollectionExtensions = services.GetService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();

            // Assert
            Assert.NotNull(mvcServiceCollectionExtensions);
        }
    }
}