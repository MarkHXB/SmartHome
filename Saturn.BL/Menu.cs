using Saturn.BL.FeatureUtils;
using Saturn.Shared;
using System.Text;

namespace Saturn.BL
{
    public class Menu
    {
        #region Constants

        private const int MAX_TASKS_CAN_RUN = 100;

        #endregion

        #region Fields

        private FeatureHandler featureHandler;
        private bool SuccessBinding = false;
        public StringBuilder StatusQueue;
        private int retryCounts = 0;

        #endregion

        #region Properties

        public bool Exit { get; set; }

        #endregion

        public Menu()
        {
            StatusQueue = new StringBuilder();
            featureHandler = VirtualBox.GetInstance(RunMode.MENU).FeatureHandler;
        }

        public async Task Run()
        {
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
                            ExitMenu();
                            break;
                        case ConsoleKey.D1:
                            RunFeature();
                            break;
                        case ConsoleKey.D2:
                            RunAllFeature();
                            break;
                        case ConsoleKey.D3:
                            Stop();
                            break;
                        case ConsoleKey.D5:
                            EnableFeature();
                            break;
                        case ConsoleKey.D6:
                            DisableFeature();
                            break;
                        case ConsoleKey.D7:
                            GetFeature();
                            break;
                        case ConsoleKey.D8:
                            GetAllFeature();
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

        private void GetAllFeature()
        {
            Console.Clear();

            foreach (var feautre in featureHandler.GetFeatures())
            {
                Console.WriteLine(feautre);
            }

            Console.WriteLine("\n\tPress a button to continue...");
            Console.ReadKey(true);
        }

        private void GetFeature()
        {
            string? featureName = GetReadLine("Name of feature you want to GET");

            var feautre = featureHandler.GetFeatures().FirstOrDefault(fe => fe.Name.Equals(featureName));
            if (feautre != null)
            {
                Console.WriteLine(feautre);
            }

            Console.WriteLine("\n\tPress a button to continue...");
            Console.ReadKey(true);
        }

        private void DisableFeature()
        {
            string? featureName = GetReadLine("Name of feature you want to DISABLE");

            Task.Run(async () => await CommandHandler.Parse(featureHandler, "disable", featureName));
        }

        private int GetWorkingThreads()
        {
            int maxThreads;
            int completionPortThreads;
            ThreadPool.GetMaxThreads(out maxThreads, out completionPortThreads);

            int availableThreads;
            ThreadPool.GetAvailableThreads(out availableThreads, out completionPortThreads);

            int count = maxThreads - availableThreads;
            if (count > 0)
            {
                count = count / 2;
            }

            return count;
        }

        private void RunFeature()
        {
            string? featureName = GetReadLine("Name of feature you want to RUN");

            Task.Run(async () => await CommandHandler.Parse(featureHandler, "run", featureName));
        }

        private static string GetReadLine(string message)
        {
            Console.Clear();

            Console.WriteLine(message);
            string? featureName = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(featureName))
            {
                featureName = Console.ReadLine();
            }

            return featureName;
        }

        private void RunAllFeature()
        {
            var tasks = new List<Task>();

            foreach (var feature in featureHandler.GetFeatures())
            {
                tasks.Add(Task.Run(async () => await CommandHandler.Parse(featureHandler, "run", feature.Name)));
            }

            Task.WhenAll(tasks);
        }

        private void EnableFeature()
        {
            string? featureName = GetReadLine("Name of feature you want to ENABLE");

            Task.Run(async () => await CommandHandler.Parse(featureHandler, "enable", featureName));
        }

        public void ExitMenu()
        {
            CommandHandler.Parse(featureHandler, "stopall");
            SetStatus("Exiting...");
            SuccessBinding = true;
            Exit = true;
        }

        public void Stop()
        {
            string? featureName = GetReadLine("Name of feature you want to STOP");

            Thread.Sleep(1000);

            CommandHandler.Parse(featureHandler, "stop", featureName);
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
            sb.AppendLine("7, get <FeatureName>");
            sb.AppendLine("8, getall");
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
