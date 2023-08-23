using System.Diagnostics;

namespace Saturn.Shared
{
    public class FeatureExecutable : Feature
    {
        public FeatureExecutable(string name, string runFilePath, List<Command> commands, bool isEnabled, Cli? cli = null, WebApi? webApi = null)
            : base(name, runFilePath, FeatureResult.Exe, commands, cli, webApi)
        {
            IsEnabled = isEnabled;
        }

        public override async Task Run(string[]? args = null)
        {
            if (IsEnabled is false)
            {
                throw new Exception($"- Disabled - You wanted to run the {Name} which is not enabled.");
            }

            if (string.IsNullOrWhiteSpace(RunFilePath))
            {
                throw new FileNotFoundException(Name + " couldn't run because file not found");
            }

            if (args is null || args?.Length == 0)
            {
                args = new string[]
                {
                    "run"
                };
            }

            var processConfig = new ProcessStartInfo
            {
                FileName = RunFilePath,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process = Process.Start(processConfig))
            {
                IsRunning = true;

                OutputDataReceived(Process);
                ErrorDataReceived(Process);

                Process.WaitForExit();

                IsRunning = false;
            }
        }
    }
}
