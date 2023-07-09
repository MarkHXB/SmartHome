using System.Diagnostics;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureExecutable : Feature
    {
        public FeatureExecutable(string featureName, string pathToExecutable, bool isEnabled) : base (featureName, FeatureResult.Exe)
        {
            IsEnabled = isEnabled;
            PathToExecutable = pathToExecutable;
        }

        public string PathToExecutable { get; }

        public override async Task Run(string[]? args = null)
        {
            if (IsEnabled is false)
            {
                throw new Exception($"You wanted to run the {FeatureName} which is not enabled.\nTry to run Enable() method.");
            }

            if (string.IsNullOrWhiteSpace(PathToExecutable))
            {
                throw new FileNotFoundException(nameof(FeatureName) + " couldn't run because file not found");
            }

            if(args is null || args.Length == 0)
            {
                args = new string[]
                {
                    "run"
                };
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = PathToExecutable,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            await Task.Run(() =>
            {
                proc.Start();

                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput?.ReadLine() ?? string.Empty;
                     
                    Output.Add(DateTime.Now,line);
                }
            }); 
        }
    }}
