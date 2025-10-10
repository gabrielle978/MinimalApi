using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Domínio.Interfaces;
using Test.Mocks;

namespace Test.Helpers;

[TestClass]
public static class Setup
{
    public static HttpClient client = default!;
    private static WebApplicationFactory<Startup> _factory = default!;

    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        _factory = new WebApplicationFactory<Startup>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IAdministradorServico, AdministradorServicoMock>();
                });
            });

        client = _factory.CreateClient();
    }

    [ClassCleanup]
    public static void ClassCleanUp()
    {
        _factory?.Dispose();
    }
}
