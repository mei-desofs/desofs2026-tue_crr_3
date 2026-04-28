
using TeaShop.Application;
using TeaShop.Infrastructure;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Seed;
using TeaShop.Infrastructure.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddTeaShopRateLimiting();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
});

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
builder.Services.AddScoped<AdminSeeder>();


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

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<TeaShopDbContext>();

    var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.Run();
