using Saturn.BL;
using Saturn.Shared;

namespace Saturn.Service
{
    public class Worker : BackgroundService
    {
        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // implement service logic here...
                await VirtualBox.GetInstance(RunMode.DAEMON).Run();

                await Task.Delay(AppInfo.DaemonDelayValueInSeconds, stoppingToken);
            }
        }
    }
}