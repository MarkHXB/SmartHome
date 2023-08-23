using Saturn.Service;
using Saturn.Shared;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<ILoggerLogicProvider>(new LoggerLogicProviderSerilog(RunMode.DAEMON));
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();