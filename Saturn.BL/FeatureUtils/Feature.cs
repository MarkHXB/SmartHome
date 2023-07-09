using Newtonsoft.Json;

namespace Saturn.BL.FeatureUtils
{
    public abstract class Feature
    {
        protected Feature(string featureName, FeatureResult featureResult, DateOnly? registrationDate = null)
        {
            FeatureName = featureName;
            RegistrationDate = registrationDate ?? DateOnly.FromDateTime(DateTime.Now);
            FeatureResult = featureResult;
            Output = new Dictionary<DateTime, string>();
        }

        public bool IsEnabled { get; protected set; }
        public string FeatureName { get; }
        public DateOnly RegistrationDate { get; private set; }
        public FeatureResult FeatureResult { get; private set; } = FeatureResult.Exe;

        [JsonIgnore]
        public IDictionary<DateTime, string> Output { get; protected set; }

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
        public abstract Task Run(string[]? args = null);

        public override string ToString()
        {
            return $"{nameof(FeatureName)} : {FeatureName}\n";
        }
        public string OutputToString()
        {
            string output = string.Empty;

            foreach (var item in Output)
            {
                output += string.Concat("[ ",item.Key," ]", " ", item.Value);
            }

            return output;
        }
    }
}
