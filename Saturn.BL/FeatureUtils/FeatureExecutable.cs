using System.Diagnostics;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureExecutable : Feature
    {
        public FeatureExecutable(string featureName, bool isEnabled, string pathToFile) : base(featureName, FeatureResult.Exe, pathToFile)
        {
            IsEnabled = isEnabled;
        }

        public override async Task Run(string[]? args = null)
        {
            if (IsEnabled is false)
            {
                throw new Exception($"- Disabled - You wanted to run the {FeatureName} which is not enabled.");
            }

            if (string.IsNullOrWhiteSpace(PathToFile))
            {
                throw new FileNotFoundException(nameof(FeatureName) + " couldn't run because file not found");
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
                FileName = PathToFile,
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
