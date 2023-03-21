using App.WindowsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using CliWrap;

const string ServiceName = ".NET Joke Service";

if (args is { Length: 1 })
{
    string executablePath =
        Path.Combine(AppContext.BaseDirectory, "App.WindowsService.exe");

    if (args[0] is "/Install")
    {
        await Cli.Wrap("sc")
            .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
            .ExecuteAsync();
    }
    else if (args[0] is "/Uninstall")
    {
        await Cli.Wrap("sc")
            .WithArguments(new[] { "stop", ServiceName })
            .ExecuteAsync();

        await Cli.Wrap("sc")
            .WithArguments(new[] { "delete", ServiceName })
            .ExecuteAsync();
    }

    return;
}

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = ".NET Joke Service";
    })
    .ConfigureServices(services =>
    {
        LoggerProviderOptions.RegisterProviderOptions<
            EventLogSettings, EventLogLoggerProvider>(services);

        services.AddSingleton<JokeService>();
        services.AddHostedService<WindowsBackgroundService>();
    })
    .ConfigureLogging((context, logging) =>
    {
        // See: https://github.com/dotnet/runtime/issues/47303
        logging.AddConfiguration(
            context.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();
