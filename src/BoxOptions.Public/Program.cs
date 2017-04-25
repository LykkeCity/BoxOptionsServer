using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BoxOptions.Public
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options => options.ThreadCount = 1)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls("http://*:5000")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
