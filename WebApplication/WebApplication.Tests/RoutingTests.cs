using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace WebApplication.Tests
{
    public class RoutingTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RoutingTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProducts_Controller_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Products from controller", content);
        }

        [Fact]
        public async Task GetProducts_MinimalApi_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/products/minimal");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Products from Minimal API", content);
        }

        [Fact]
        public async Task GetProducts_MinimalApiWithId_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/products/minimal/123");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Product 123 from Minimal API", content);
        }

        [Fact]
        public async Task GetProducts_GroupEndpoint_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/products/group/");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Products group", content);
        }

        [Fact]
        public async Task GetProducts_GroupEndpointWithId_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/api/products/group/456");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Product 456 from group", content);
        }
    }
}
