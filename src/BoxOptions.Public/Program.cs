using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BoxOptions.Public
{
    public class Program
    {
        public const string Name = "BoxOptions.Public";

        public static void Main(string[] args)
        {
            var cfgBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.dev.json", true, true)
                .AddEnvironmentVariables();

            var configuration = cfgBuilder.Build();

            int kestrelThreadsCount = 1;
            string threadsCount = configuration["KestrelThreadCount"];

            if (threadsCount != null)
            {
                if (!int.TryParse(threadsCount, out kestrelThreadsCount))
                {
                    Console.WriteLine($"Can't parse KestrelThreadsCount value '{threadsCount}'");
                    return;
                }
            }

            Console.WriteLine($"Kestrel threads count: {kestrelThreadsCount}");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls("http://*:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
