using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using WallpaperNormaliser.ConsoleUi.Bootstrap;
using WallpaperNormaliser.ConsoleUi.Navigation;

namespace WallpaperNormaliser.ConsoleUi;

public class Program
{
    public static async Task Main(string[] args)
    {
        IServiceCollection services = new ServiceCollection();
        ServiceRegistration.Configure(services);

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        
        StartupRunner runner = scope.ServiceProvider.GetRequiredService<StartupRunner>();
        await runner.RunAsync(provider);
    }
}
