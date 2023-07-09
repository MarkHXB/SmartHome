namespace Saturn.BL.Logging
{
    using Serilog.Core;

    public class LoggerLogicProviderSerilog : ILoggerLogicProvider
    {
        Logger logger;

        public LoggerLogicProviderSerilog(Logger logger)
        {
            this.logger = logger;
        }

        public void LogInformation(string message)
        {
            logger.Information(message);
        }

        public void LogWarning(string message)
        {
            logger.Warning(message);
        }
    }
}
