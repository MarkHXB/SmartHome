using Saturn.BL;
using Saturn.BL.Persistence;
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
                await VirtualBox.GetInstance(RunMode.DAEMON).Run();

                await Task.Delay(AppInfo.DaemonDelayValueInSeconds, stoppingToken);
            }
        }
    }
}