using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TaleOf2Cities.Interface;

namespace TaleOf2Cities
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            RegisterServices();
            IServiceScope scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<FileParserConsole>().Run();
            DisposeServices();
           
        }

        private static void RegisterServices()
        {
            var services = new ServiceCollection();
            var taleConfig = new TaleConfiguration();
            IConfiguration talefiguration = new ConfigurationBuilder()
               .AddJsonFile("talesettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables()               
               .Build();

            talefiguration.Bind(TaleConfiguration.Position, taleConfig);
            services.AddSingleton(taleConfig);
            services.AddSingleton<IFileParseReportGenerator, FileParserReportGenerator>();
            services.AddSingleton<FileParserConsole>();

            _serviceProvider = services.BuildServiceProvider(true);
        }
        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
