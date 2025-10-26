using GlobalIO.Infrastructure.HealthChecks;
using GlobalIO.Infrastructure.Interfaces;
using GlobalIO.Infrastructure.Models;
using GlobalIO.Infrastructure.PubSub;
using GlobalIO.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            EnableSupportForUnicode();

            AnsiConsole.Write(
            new FigletText("Pub/Sub Manager")
                .LeftJustified()
                .Color(Color.Blue));

            var host = CreateHostBuilder(args).Build();

            var settings = host.Services.GetRequiredService<IOptions<GooglePubSubSettings>>().Value;
            AnsiConsole.MarkupLine($"[green]{Emoji.Known.CheckMarkButton} Configured for project: {settings.ProjectId}[/]");
            AnsiConsole.MarkupLine($"[grey]Emulator: {settings.EmulatorHost} (Use Emulator: {settings.UseEmulator})[/]");
            AnsiConsole.WriteLine();

            var commander = host.Services.GetRequiredService<PubSubCommander>();
            await commander.ExecuteAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            AnsiConsole.MarkupLine($"[red]{Emoji.Known.CrossMark} The application has encountered an error and will be closed.[/]");
            AnsiConsole.MarkupLine($"[grey]{Emoji.Known.PersonWalking} Press any key to exit...[/]");
            Console.ReadKey();
        }
    }
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);
        })
        .ConfigureServices((hostContext, services) =>
            {
                var builder = Host.CreateDefaultBuilder(args);

                services.Configure<GooglePubSubSettings>(
                    hostContext.Configuration.GetSection(GooglePubSubSettings.SectionName));

                services.AddSingleton<IPubSubService, PubSubService>();
                services.AddSingleton<PubSubCommander>();

                services.AddHealthChecks()
                    .AddCheck<PubSubHealthCheck>("pubsub");
            }).UseConsoleLifetime(opts =>
            {
                opts.SuppressStatusMessages = true;
            });

    private static void EnableSupportForUnicode()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        AnsiConsole.WriteLine($"Unicode support: {AnsiConsole.Profile.Capabilities.Unicode}");
        AnsiConsole.WriteLine($"Color support: {AnsiConsole.Profile.Capabilities.ColorSystem}");
    }
}