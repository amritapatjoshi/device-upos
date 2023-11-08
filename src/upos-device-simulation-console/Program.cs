using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace UposDeviceSimulationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder()
           .ConfigureServices(ConfigureServices)
           .Build()
           .Services
           .GetService<PosExecutor>()
           .Execute();
        }
        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.ConfigureServices();
        }

    }
}
