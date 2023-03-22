using App.WindowsService;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

const string ServiceName = ".NET Joke Service";

switch (args)
{
    case ["/Install"]:
        var bin = Path.Combine(AppContext.BaseDirectory, ServiceName + ".exe");
        await sc("create", ServiceName, $"binPath={bin}", "start=auto");
        await sc("start", ServiceName);
        return;
    case ["/Uninstall"]:
        try { await sc("stop", ServiceName); } catch { }
        await sc("delete", ServiceName);
        return;
        static Task sc(params string[] args) => CliWrap.Cli.Wrap("sc").WithArguments(args).ExecuteAsync();
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
