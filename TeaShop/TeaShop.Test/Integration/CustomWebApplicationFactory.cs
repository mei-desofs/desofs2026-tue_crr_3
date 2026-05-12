using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using TeaShop.Application.Auth;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public ITeaRepository TeaRepositoryMock { get; } = Substitute.For<ITeaRepository>();
    public IPasswordPolicyChecker PasswordPolicyCheckerMock { get; } = Substitute.For<IPasswordPolicyChecker>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("detailedErrors", "true");
        builder.CaptureStartupErrors(true);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITeaRepository>();
            services.RemoveAll<IPasswordPolicyChecker>();

            services.AddSingleton(TeaRepositoryMock);
            services.AddSingleton(PasswordPolicyCheckerMock);

            PasswordPolicyCheckerMock.IsValidAsync(Arg.Any<string>())
                .Returns(true);
        });
    }
}