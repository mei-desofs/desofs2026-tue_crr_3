
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using TeaShop.Domain.Catalog;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.IntegrationTests;
using Xunit;

namespace TeaShop.Test.Integration;

public class CatalogTests : IClassFixture<CustomWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public CatalogTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturn200_AndList()
    {
        var response = await _client.GetAsync("/api/catalog");
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
       
            throw new Exception($"Server crashed! Error: {errorContent}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
      

        content.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_Invalid_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/catalog/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ValidFlow_ShouldNotCrash()
    {
        var listResponse = await _client.GetAsync("/api/catalog");

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync($"/api/catalog/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GetAll_WithCategoryFilter_ShouldReturn200()
    {
        var categoryId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/catalog?categoryId={categoryId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}