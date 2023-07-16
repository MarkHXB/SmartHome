using Newtonsoft.Json;
using System.Diagnostics;

namespace Saturn.BL.FeatureUtils
{
    public abstract class Feature
    {
        protected Feature(string featureName, FeatureResult featureResult, string pathToFile, CancellationTokenSource cancellationTokenSource, DateOnly? registrationDate = null)
        {
            FeatureName = featureName;
            RegistrationDate = registrationDate ?? DateOnly.FromDateTime(DateTime.Now);
            FeatureResult = featureResult;
            Output = new Dictionary<DateTime, string>();
            PathToFile = pathToFile;
            CancellationTokenSource = cancellationTokenSource;
        }

        public bool IsEnabled { get; protected set; }
        public string FeatureName { get; }
        public DateOnly RegistrationDate { get; private set; }
        public FeatureResult FeatureResult { get; private set; } = FeatureResult.Exe;
        public string PathToFile { get; protected set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public event EventHandler CancellationRequested;

        /// <summary>
        /// This is needed due to CancellationRequested event. If the process ends then should unsubscribe events.
        /// </summary>
        public event EventHandler OperationFinished;

        [JsonIgnore]
        public IDictionary<DateTime, string> Output { get; protected set; }

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
                output += string.Concat("[ ", item.Key, " ]", " ", item.Value);
            }

            return output;
        }
        public void Stop()
        {
            CancellationRequested?.Invoke(this, EventArgs.Empty);
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
            OperationFinished?.Invoke(this, EventArgs.Empty);
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
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (e, d) =>
            {
                string data = string.Join(" ", "[ ERROR ]", d.Data ?? string.Empty);
                Output.Add(DateTime.Now, data);
            };
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{nameof(FeatureName)} : {FeatureName}\n";
        }

        #endregion
    }
}
