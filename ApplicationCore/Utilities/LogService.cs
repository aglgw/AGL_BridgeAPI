using Microsoft.Extensions.Configuration;
using Serilog;

namespace AGL.Api.ApplicationCore.Utilities
{
    public static class LogService
    {
        public static void InitLogger()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();


            //var logFilePath = configuration["Serilog:LogFilePath"];
            //Log.Logger = new LoggerConfiguration()
            //    //.MinimumLevel.Verbose()
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            //    .ReadFrom.Configuration(configuration)
            //    .CreateLogger();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }


        public static void logVerbose(string message)
        {
            InitLogger();
            Log.Verbose(message);
            Log.CloseAndFlush();
        }

        public static void logDebug(string message)
        {
            InitLogger();
            Log.Debug(message);
            Log.CloseAndFlush();
        }

        public static void logInformation(string message)
        {
            InitLogger();
            Log.Information(message);
            Log.CloseAndFlush();
        }

        public static void logWarning(string message)
        {
            InitLogger();
            Log.Warning(message);
            Log.CloseAndFlush();
        }

        public static void logError(string message)
        {
            InitLogger();
            Log.Error(message);
            Log.CloseAndFlush();
        }

        public static void logFatal(string message)
        {
            InitLogger();
            Log.Fatal(message);
            Log.CloseAndFlush();
        }

    }
}