namespace Saturn.API.Logging
{
    using Microsoft.Extensions.Logging;
    using Saturn.BL.Logging;

    public class LoggerLogicProviderMicrosoft : ILoggerLogicProvider
    {
        private readonly ILogger logger;

        public LoggerLogicProviderMicrosoft(ILogger logger)
        {
            this.logger = logger;
        }

        public void LogInformation(string message)
        {
            logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            logger.LogWarning(message);
        }
    }
}
