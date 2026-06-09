
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Application;
using TeaShop.Infrastructure;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Middleware;
using TeaShop.Infrastructure.Persistence;
using TeaShop.Infrastructure.Persistence.Seed;
using TeaShop.Infrastructure.Security;

[assembly:ApiController]
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

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); 
    options.Lockout.MaxFailedAccessAttempts = 5; 
    options.Lockout.AllowedForNewUsers = true; 
});

builder.Services.AddScoped<AdminSeeder>();


var app = builder.Build();

app.UseMiddleware<GenericExceptionMiddleware>();

var policyCollection = new HeaderPolicyCollection()
    .AddDefaultSecurityHeaders()
    .AddCustomHeader("Cross-Origin-Resource-Policy", "same-origin");

app.UseSecurityHeaders(policyCollection);


if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("CI"))
{
    app.UseDeveloperExceptionPage();

    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Tea Shop API v1");
    });
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}





app.UseRateLimiter();

app.UseInfrastructureMiddleware();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<TeaShopDbContext>();

    var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}


await app.RunAsync();
