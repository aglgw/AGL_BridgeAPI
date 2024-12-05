using AGL.Api.Bridge_API;
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using System;
using System.IO;
using System.Net;

namespace AGL.Api.Bridge_API
{

    public class Program
    {
        public static void Main(string[] args)
        {

            //Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Main"); // 환경 강제 설정
            var configuration = GetConfiguration();
            //Log.Logger = CreateSerilogLogger(configuration);
            try
            {
                //Log.Information("Configuring web host...");
                LogService.logInformation("Configuring web host...");
                var builder = BuildWebHost(configuration, args);



                //Log.Information("Starting web host...");
                LogService.logInformation("Starting web host...");
                builder.Run();
            }
            catch (Exception ex)
            {
                //Log.Fatal(ex, "Program terminated  unexpectedly!");
                LogService.logError($"Program terminated  unexpectedly! - {ex.Message}");
            }
            //finally
            //{
            //    Log.CloseAndFlush();
            //}



        }
        public static IWebHost BuildWebHost(IConfiguration configuration, string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                        .CaptureStartupErrors(false)
                        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
                        .UseStartup<Startup>()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .Build();
        }

        public static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
        {
            var logFilePath = configuration["Serilog:LogFilePath"];
            return new LoggerConfiguration()
                //.MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        public static IConfiguration GetConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
        public static IHost CreateHostBuilder(IConfiguration configuration, string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .Build();
        }
    }
}