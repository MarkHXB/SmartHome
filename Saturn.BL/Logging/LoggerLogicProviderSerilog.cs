namespace Saturn.BL.Logging
{
    using Serilog;
    using Serilog.Core;

    public class LoggerLogicProviderSerilog : ILoggerLogicProvider
    {
        Logger _logger;

        public LoggerLogicProviderSerilog()
        {
            var loggerConfiguration = new LoggerConfiguration()
             .WriteTo.File(AppInfo.LogFilePath_CLI);

            _logger = loggerConfiguration.CreateLogger();
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
