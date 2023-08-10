namespace Saturn.BL.Logging
{
    using Saturn.BL.AppConfig;
    using Serilog;
    using Serilog.Core;

    public class LoggerLogicProviderSerilog : ILoggerLogicProvider
    {
        Logger _logger;

        public LoggerLogicProviderSerilog()
        {
            _logger  = new LoggerConfiguration()
             .WriteTo.File(AppInfo.LogFilePath_CLI)
             .Enrich.FromLogContext()
                .CreateLogger();
        }
        public LoggerLogicProviderSerilog(Logger logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.Information(message);
        }

        public void LogWarning(string message)
        {
            _logger.Warning(message);
        }
    }
}
