namespace Saturn.Shared
{
    using Serilog;
    using Serilog.Core;

    public class LoggerLogicProviderSerilog : ILoggerLogicProvider
    {
        private Logger _logger;

        public LoggerLogicProviderSerilog(RunMode runMode)
        {
            string logFile = string.Empty;
            switch (runMode)
            {
                case RunMode.CLI: logFile = AppInfo.LogFilePath_CLI; break;
                case RunMode.WEBAPI: logFile = AppInfo.LogFilePath_WEBAPI; break;
                case RunMode.DAEMON: logFile = AppInfo.LogFilePath_DAEMON; break;
                case RunMode.MAUI: logFile = AppInfo.LogFilePath_MAUI; break;
                case RunMode.MENU: logFile = AppInfo.LogFilePath_MENU; break; 
                default: throw new Exception("You should enter the platform you use.");
            }

            _logger  = new LoggerConfiguration()
             .WriteTo.File(logFile)
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
