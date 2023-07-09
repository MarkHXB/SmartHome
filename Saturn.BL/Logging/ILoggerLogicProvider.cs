namespace Saturn.BL.Logging
{
    public interface ILoggerLogicProvider
    {
         void LogInformation(string message);

         void LogWarning(string message);
    }
}
