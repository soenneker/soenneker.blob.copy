using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Soenneker.Blob.Copy.Registrars;
using Soenneker.Fixtures.Unit;
using Soenneker.Utils.Test;

namespace Soenneker.Blob.Copy.Tests;

public class Fixture : UnitFixture
{
    public override System.Threading.Tasks.ValueTask InitializeAsync()
    {
        SetupIoC(Services);

        return base.InitializeAsync();
    }

    private static void SetupIoC(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        IConfiguration config = TestUtil.BuildConfig();
        services.AddSingleton(config);
        services.AddBlobCopyAsScoped();
    }
}
