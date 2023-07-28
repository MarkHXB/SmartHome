using Saturn.BL;
using Saturn.BL.FeatureUtils;
using Saturn.BL.Logging;
using System.Text;
using System.Threading;

namespace Saturn.CLI
{
    public class Menu
    {
        #region Constants

        private const int MAX_TASKS_CAN_RUN = 100;

        #endregion

        #region Fields

        private bool SuccessBinding = false;
        public StringBuilder StatusQueue;
        private FeatureHandler m_FeatureHandler;
        private ILoggerLogicProvider m_LoggerLogicProvider;
        private int retryCounts = 0;

        #endregion

        #region Properties

        public bool Exit { get; set; }

        #endregion

        public Menu()
        {
            StatusQueue = new StringBuilder();
            m_LoggerLogicProvider = new LoggerLogicProviderSerilog();
        }

        public async Task Run()
        {
            m_FeatureHandler = await FeatureHandler.BuildAsync(m_LoggerLogicProvider);

            while (!Exit)
            {
                Refresh();

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.Q:
                            Stop();
                            break;
                        case ConsoleKey.D1:
                            await RunFeature();
                            break;
                        case ConsoleKey.D2:
                            await RunAllFeature();
                            break;
                        default:
                            SetStatus("Please enter a valid menu option...");
                            SuccessBinding = false;
                            retryCounts++;
                            break;
                    }
                }

                Thread.Sleep(500);
            }
        }
        private int GetWorkingThreads()
        {
            int maxThreads;
            int completionPortThreads;
            ThreadPool.GetMaxThreads(out maxThreads, out completionPortThreads);

            int availableThreads;
            ThreadPool.GetAvailableThreads(out availableThreads, out completionPortThreads);

            int count = ((maxThreads - availableThreads) - 1);
            if (count > 0)
            {
                count = count / 2;
            }

            return count;
        }
        private async Task RunFeature()
        {
            Console.Clear();

            Console.Write("Enter the name of feature you want to run: ");

            string? featureName = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(featureName))
            {
                featureName = Console.ReadLine();
            }

            await CommandHandler.Parse(m_FeatureHandler, "run", featureName);
        }
        private async Task RunAllFeature()
        {
            await CommandHandler.Parse(m_FeatureHandler, new string[] { "runall" });
        }
        public void Stop()
        {
            m_FeatureHandler.StopAll();
            SetStatus("Exiting...");
            SuccessBinding = true;
            Exit = true;
        }
        private void SetStatus(string message)
        {
            StatusQueue.Append(string.Concat(Environment.NewLine, message, Environment.NewLine));
        }
        private void Refresh()
        {
            Console.Clear();
            Console.Write(MenuToString());
        }
        private string MenuToString()
        {
            return string.Concat(Header(), Body(), Footer());
        }
        private string Header()
        {
            int padding = 30;
            string dasher = new string('-', padding);

            return string.Join(Environment.NewLine, dasher, $"\tWaiting tasks: {0}", $"\tRunning tasks: {GetWorkingThreads()}", dasher);
        }
        private string Body()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Environment.NewLine);
            sb.AppendLine("1, Run <Name>");
            sb.AppendLine("2, RunAll");
            sb.AppendLine("3, Stop <Name>");
            // sb.AppendLine("3, RunWhen <Name> <When>"); Coming...
            sb.AppendLine("4, Add <Solution file path>|<Exe file path>");
            sb.AppendLine("5, Enable <Name>");
            sb.AppendLine("6, Disable <Name>");
            sb.AppendLine($"{Environment.NewLine}Esq / q, Exit");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
        private string Footer()
        {
            if (SuccessBinding)
            {
                string tmp = StatusQueue.ToString();
                StatusQueue.Clear();
                return tmp;
            }
            else
            {
                if (retryCounts >= 2)
                {
                    StatusQueue.Clear();
                    retryCounts = 0;
                }
                return StatusQueue.ToString();
            }
        }
    }
}
