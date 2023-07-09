using System.Reflection;

namespace Saturn.BL.FeatureUtils
{
    public class FeatureDll : Feature
    {
        private string dllName = string.Empty;

        public FeatureDll(string featureName, string dllName, bool isEnabled) : base(featureName, FeatureResult.Dll)
        {
            DllName = dllName;
            IsEnabled = isEnabled;
        }
        public FeatureDll(string featureName, string dllName, Type program, bool isEnabled) : base(featureName, FeatureResult.Dll)
        {
            DllName = dllName;
            Program = program;
            IsEnabled = isEnabled;
        }

        public string DllName
        {
            get
            {
                return dllName;
            }
            private set
            {
                dllName = value.Replace(".dll", "");
            }
        }
        public Type? Program { get; }

        public override async Task Run(string[]? args = null)
        {
            if (IsEnabled is false)
            {
                throw new Exception($"You wanted to run the {DllName} which is not enabled.\nTry to run Enable() method.");
            }

            _ = Program ?? throw new ArgumentNullException(nameof(Program));

            if (args is null)
            {
                await Task.Run(() =>
                {
                    Program.InvokeMember("Main", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { "asd" });
                });
            }
            else
            {
                await Task.Run(() =>
                {
                    Program.InvokeMember("Main", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, args);
                });
            }
        }
    }
}
