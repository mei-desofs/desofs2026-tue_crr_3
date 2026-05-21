using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TeaShop.Application.Orders.DTOs;
using TeaShop.Domain.Orders;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.IntegrationTests;
using Xunit;

namespace TeaShop.Test.Integration;

public class OrderTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    public IOrderRepository _OrderRepositoryMock { get; } = Substitute.For<IOrderRepository>();


    public OrderTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IOrderRepository>();
                services.AddSingleton(_OrderRepositoryMock);
            });
        }).CreateClient();
    }


    [Fact]
    public async Task Export_WithoutAuthentication_ShouldReturn401()
    {
        var request = new ExportSalesReportRequest(
            "Security_Test",
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/orders/export", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Export_AsStandardUser_ShouldReturn403Forbidden()
    {
        var request = new ExportSalesReportRequest(
            "Security_Test",
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");


        var userSession = CustomWebApplicationFactory.CreateMockSession(Guid.NewGuid(), Roles.Customer);

        _factory.SessionRepositoryMock
            .FindByTokenAsync("user-token", Arg.Any<CancellationToken>())
            .Returns(userSession);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await _client.PostAsync("/api/orders/export", content);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task Export_AsAdmin_WithValidRequest_ShouldReturn200AndFileDownload()
    {
        var request = new ExportSalesReportRequest(
            "Sales Report Q1",
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");



        var orderUserId = Guid.NewGuid();
        var teaId = Guid.NewGuid();

        var orderItem = OrderItem.Create(teaId, 2, 15.50m);
        var mockOrder = Order.Create(orderUserId, [orderItem]);

        var ordersInDatabase = new List<Order> { mockOrder };

        _OrderRepositoryMock.GetOrdersInDateRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(ordersInDatabase);



        var adminSession = CustomWebApplicationFactory.CreateMockSession(Guid.NewGuid(), Roles.Admin);

        _factory.SessionRepositoryMock
            .FindByTokenAsync("admin-token", Arg.Any<CancellationToken>())
            .Returns(adminSession);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await _client.PostAsync("/api/orders/export", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");

        // Verify filename formatting in the download header
        var contentDisposition = response.Content.Headers.ContentDisposition;
        contentDisposition.Should().NotBeNull();
        contentDisposition!.DispositionType.Should().Be("attachment");
        contentDisposition.FileName.Should().Contain("Sales Report Q1_");
        contentDisposition.FileName.Should().EndWith(".csv");


        var fileBytes = await response.Content.ReadAsByteArrayAsync();
        var csvText = Encoding.UTF8.GetString(fileBytes);

        csvText.Should().Contain("OrderId,UserId,Status,CreatedAt,TotalAmount");
        csvText.Should().Contain(mockOrder.Id.ToString());
        csvText.Should().Contain(orderUserId.ToString());
        csvText.Should().Contain("Pending"); 
        csvText.Should().Contain("31.00");  
    }
}