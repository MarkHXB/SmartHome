using System.Diagnostics;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureDll : Feature
    {
        public FeatureDll(string featureName, bool isEnabled, string pathToFile) : base(featureName, FeatureResult.Dll, pathToFile)
        {
            IsEnabled = isEnabled;
        }

        public override async Task Run(string[]? args = null)
        {
            if (IsEnabled is false)
            {
                throw new Exception($"You wanted to run the {FeatureName} which is not enabled.\nTry to run Enable() method.");
            }

            if (string.IsNullOrWhiteSpace(PathToFile))
            {
                throw new FileNotFoundException(nameof(FeatureName) + " couldn't run because file not found");
            }

            string arg = string.Join(" ", PathToFile, args ?? Array.Empty<string>());

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
