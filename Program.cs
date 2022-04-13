using BimsyncCLI.Services;
using BimsyncCLI.Services.DelegatingHandlers;
using BimsyncCLI.Services.HttpServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BimsyncCLI
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + "\\appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(Configuration)
                   .Enrich.FromLogContext()
                   .CreateLogger();

            ICredentials credentials = CredentialCache.DefaultCredentials;
            IWebProxy proxy = WebRequest.DefaultWebProxy;
            proxy.Credentials = credentials;

            SettingsService settingsService = new SettingsService("settingsCLI.bimsync");

            AuthenticationService authenticationService = new AuthenticationService(proxy, settingsService);

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(config =>
                    {
                        config.ClearProviders();
                        config.AddProvider(new SerilogLoggerProvider(Log.Logger));
                    });
                    services.AddHttpClient();

                    services.AddHttpClient<IBimsyncClient, BimsyncClient>()
                .AddHttpMessageHandler(handler => new AuthenticationDelegatingHandler(authenticationService))
                .ConfigurePrimaryHttpMessageHandler(handler =>
                   new HttpClientHandler()
                   {
                       Proxy = proxy,
                       AutomaticDecompression = System.Net.DecompressionMethods.GZip
                   });

                    services.AddSingleton<AuthenticationService>(y =>
 {
     return authenticationService;
 });

                    services.AddSingleton<SettingsService>(y =>
                    {
                        return settingsService;
                    });
                });

            try
            {
                return await builder.RunCommandLineApplicationAsync<bimsyncCmd>(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
