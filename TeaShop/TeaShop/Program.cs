
using TeaShop.Application;
using TeaShop.Infrastructure;
using TeaShop.Infrastructure.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddTeaShopRateLimiting();

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("CustomerOrAbove", p =>
        p.RequireAuthenticatedUser()
         .RequireRole("CUSTOMER", "MANAGER", "ADMIN"));

    opts.AddPolicy("ManagerOrAbove", p =>
        p.RequireAuthenticatedUser()
         .RequireRole("MANAGER", "ADMIN"));

    opts.AddPolicy("AdminOnly", p =>
        p.RequireAuthenticatedUser()
         .RequireRole("ADMIN"));
});


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Tea Shop API v1");
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseRateLimiter();

app.UseInfrastructureMiddleware();

app.UseAuthorization();
app.MapControllers();

app.Run();
