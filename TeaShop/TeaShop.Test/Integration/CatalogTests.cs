
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Text;
using System.Text.Json;
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

    [Fact]
    public async Task Create_WithoutAuthentication_ShouldReturn401()
    {
        var request = new
        {
            name = "Green Tea",
            price = 10,
            stock = 5,
            categoryId = Guid.NewGuid()
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/catalog", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task Update_WithoutAuthentication_ShouldReturn401()
    {
       var request = new
       {
           name = "Updated Tea",
           price = 12,
           stock = 8,
           categoryId = Guid.NewGuid()
       };

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PutAsync($"/api/catalog/{Guid.NewGuid()}", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task Delete_WithoutAuthentication_ShouldReturn401()
    {
        var response = await _client.DeleteAsync($"/api/catalog/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetImage_WhenNoImageExists_ShouldReturn404()
    {
        var nonExistentTeaId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/catalog/{nonExistentTeaId}/image");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadImage_WithoutAuthentication_ShouldReturn401()
    {
        var teaId = Guid.NewGuid();
        using var form = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }); // Dummy PNG bytes
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        form.Add(fileContent, "file", "test.png");

        var response = await _client.PostAsync($"/api/catalog/{teaId}/image", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteImage_WithoutAuthentication_ShouldReturn401()
    {
        var teaId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/catalog/{teaId}/image");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


}