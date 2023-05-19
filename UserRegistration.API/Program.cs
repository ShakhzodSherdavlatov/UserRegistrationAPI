using DbTools.DataBase;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

#pragma warning disable CS1591
namespace UserRegistration.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().MigrateDatabase().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
    }
}
