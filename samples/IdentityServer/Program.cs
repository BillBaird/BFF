// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;

namespace IdentityServerHost
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var logTheme = new AnsiConsoleTheme((IReadOnlyDictionary<ConsoleThemeStyle, string>) new Dictionary<ConsoleThemeStyle, string>()
            {
                [ConsoleThemeStyle.Text] = "\x001B[38;5;0232m",
                [ConsoleThemeStyle.SecondaryText] = "\x001B[38;5;0m",
                [ConsoleThemeStyle.TertiaryText] = "\x001B[38;5;2m",
                [ConsoleThemeStyle.Invalid] = "\x001B[33;1m",
                [ConsoleThemeStyle.Null] = "\x001B[38;5;0038m",
                [ConsoleThemeStyle.Name] = "\x001B[38;5;4m",
                [ConsoleThemeStyle.String] = "\x001B[38;5;9m",
                [ConsoleThemeStyle.Number] = "\x001B[38;5;151m",
                [ConsoleThemeStyle.Boolean] = "\x001B[38;5;0038m",
                [ConsoleThemeStyle.Scalar] = "\x001B[38;5;0079m",
                [ConsoleThemeStyle.LevelVerbose] = "\x001B[38;5;25m",
                [ConsoleThemeStyle.LevelDebug] = "\x001B[38;5;21m",
                [ConsoleThemeStyle.LevelInformation] = "\x001B[38;5;21;1m",
                [ConsoleThemeStyle.LevelWarning] = "\x001B[38;5;0229m",
                [ConsoleThemeStyle.LevelError] = "\x001B[38;5;0197m\x001B[48;5;0238m",
                [ConsoleThemeStyle.LevelFatal] = "\x001B[38;5;0197m\x001B[48;5;0238m"
            });
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.ffff} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: logTheme)
                .CreateLogger();

            try
            {
                Log.Information("Starting host...");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}