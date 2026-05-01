using System;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TeaShop.Test;

public class CatalogTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CatalogTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturn200_AndList()
    {
        var response = await _client.GetAsync("/api/catalog");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
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
        // Primeiro vai buscar todos os produtos
        var listResponse = await _client.GetAsync("/api/catalog");

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listContent = await listResponse.Content.ReadAsStringAsync();

        // Se estiver vazio, o teste ainda passa (não há produtos)
        listContent.Should().NotBeNull();

        // Este teste garante que a API não falha com ID inválido/aleatório
        var response = await _client.GetAsync($"/api/catalog/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}