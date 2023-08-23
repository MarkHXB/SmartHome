using Saturn.BL;
using Saturn.Shared;

namespace Saturn.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILoggerLogicProvider _logger;

        public Worker(ILoggerLogicProvider logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // implement service logic here...
                // shedule features goes here as well...
                await VirtualBox.GetInstance(RunMode.DAEMON).Run();

                await Task.Delay(AppInfoResolver.GetDelayValueToDaemonThread(), stoppingToken);
            }
        }
    }
}