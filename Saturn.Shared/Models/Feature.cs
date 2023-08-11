using Newtonsoft.Json;
using System.Diagnostics;

namespace Saturn.BL.FeatureUtils
{
    public abstract class Feature
    {
        protected Feature(string featureName, FeatureResult featureResult, string pathToFile, DateOnly? registrationDate = null)
        {
            FeatureName = featureName;
            RegistrationDate = registrationDate ?? DateOnly.FromDateTime(DateTime.Now);
            FeatureResult = featureResult;
            Output = new Dictionary<DateTime, string>();
            PathToFile = pathToFile;
        }

        public bool IsEnabled { get; protected set; }
        public string FeatureName { get; }
        public DateOnly RegistrationDate { get; private set; }
        public FeatureResult FeatureResult { get; private set; } = FeatureResult.Exe;
        public string PathToFile { get; protected set; }

        public event EventHandler CancellationRequested;

        [JsonIgnore]
        public IDictionary<DateTime, string> Output { get; protected set; }

        [JsonIgnore]
        public bool IsRunning { get; protected set; }

        [JsonIgnore]
        protected Process Process { get; set; }

        #region Public methods

        public void Enable()
        {
            if (IsEnabled is true)
            {
                throw new FeatureAlreadyEnabledException(ToString());
            }

            IsEnabled = true;
        }
        public void Disable()
        {
            if (IsEnabled is false)
            {
                throw new FeatureAlreadyDisabledException(ToString());
            }

            IsEnabled = false;
        }
        public string OutputToString()
        {
            string output = string.Empty;

            foreach (var item in Output)
            {
                output += string.Concat("[ ", item.Key, " ]", " ", item.Value, Environment.NewLine);
            }

            return output;
        }
        public void Stop()
        {
            CancellationRequested?.Invoke(this, EventArgs.Empty);

            if (IsRunning)
            {
                Process.CloseMainWindow();
                TryToKillProcessForce();
            }
        }

        #endregion

        #region Abstract methods

        public abstract Task Run(string[]? args = null);

        #endregion

        #region Protected methods

        protected virtual void OutputDataReceived(Process process)
        {
            process.BeginOutputReadLine();

            process.OutputDataReceived += (e, d) =>
            {
                Output.Add(DateTime.Now, d.Data ?? string.Empty);
            };
        }

        protected virtual void ErrorDataReceived(Process process)
        {
            process.BeginErrorReadLine();

            process.ErrorDataReceived += (e, d) =>
            {
                string data = string.Join(" ", "[ ERROR ]", d.Data ?? string.Empty);
                Output.Add(DateTime.Now, data);
            };
        }

        private void TryToKillProcessForce()
        {
            if (Process is null)
            {
                return;
            }

            var inf = new ProcessStartInfo()
            {
                FileName = "taskkill.exe",
                Arguments = $"/F /PID {Process.Id}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

            };
            using (var process = Process.Start(inf))
            {
                process.WaitForExit(1500);
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{FeatureName} : [\n" +
                $"\tIs Running: {IsRunning}\n" +
                 $"\tRegistration Date: {RegistrationDate}\n" +
                  $"\tType: {FeatureResult}\n" +
                   $"\tIs Enabled: {IsEnabled}\n],\n";
        }

        #endregion
    }
}
