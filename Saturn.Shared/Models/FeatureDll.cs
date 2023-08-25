using System.Diagnostics;

namespace Saturn.Shared
{
    public class FeatureDll : Feature
    {
        public FeatureDll(string name, string runFilePath, List<Command> commands, bool isEnabled, AdvancedConfig? advancedConfig = null, WebApi? webApi = null) 
            : base(name, runFilePath, FeatureResult.Dll, commands, advancedConfig, webApi)
        {
            IsEnabled = isEnabled;
        }

        public override async Task Run(string[]? args = null)
        {
            if (IsEnabled is false)
            {
                throw new Exception($"You wanted to run the {Name} which is not enabled.\nTry to run Enable() method.");
            }

            if (string.IsNullOrWhiteSpace(RunFilePath))
            {
                throw new FileNotFoundException(nameof(Name) + " couldn't run because file not found");
            }

            string arg = string.Join(" ", RunFilePath, args ?? Array.Empty<string>());

            var processConfig = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arg,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processConfig))
            {
                IsRunning = true;

                OutputDataReceived(process);
                ErrorDataReceived(process);

                process.WaitForExit();

                IsRunning = false;
            }

        }
    }
}
