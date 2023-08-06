using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace Saturn.BL.AppConfig
{
    public class ConfigHandler
    {
        private const string _saveMessage = " @Config ConfigHandler saved.";
        private const string _loadMessage = " @Config ConfigHandler loaded.";
        private const string _restoreMessageStart = " @Config ConfigHandler restoring started.";
        private const string _restoreMessageFinish = " @Config ConfigHandler restoring finished.";
        private const string _buildMessage = " @Config ConfigHandler successfully built.";
        private static bool _initialized = false;

        private static Action<string> m_logInformation;

        public static void Build(Action<string> logInformation)
        {
            m_logInformation = logInformation;

            if (_initialized)
            {
                if (ShouldUpdateConfigFile())
                {
                    Save().GetAwaiter().GetResult();
                }
                return;
            }

            if (!IsConfigFileValid())
            {
                RestoreConfigFile();
            }

            Load().GetAwaiter().GetResult();

            _initialized = true;

            m_logInformation(_buildMessage);
        }

        public static async Task Load()
        {
            string raw = await File.ReadAllTextAsync(AppInfo.ConfigFilePath);

            // FOR WINDOWS
            if (AppInfo.IsWindows)
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                foreach (var original in typeof(AppInfo_Windows).GetRuntimeFields())
                {
                    foreach (var latest in data)
                    {
                        if (original.Name.Equals(latest.Key))
                        {
                            original.SetValue(latest, latest.Value);
                        }
                    }
                }
            }
            // FOR LINUX
            else
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
                foreach (var original in typeof(AppInfo_Linux).GetRuntimeFields())
                {
                    foreach (var latest in data)
                    {
                        if (original.Name.Equals(latest.Key))
                        {
                            original.SetValue(latest, latest.Value);
                        }
                    }
                }
            }

            m_logInformation(_loadMessage);
        }

        public static async Task Save()
        {
            string data = "[]";

            var fieldDictionary = new Dictionary<string, object>();

            if (AppInfo.IsWindows)
            {
                foreach (var field in typeof(AppInfo_Windows).GetRuntimeFields())
                {
                    fieldDictionary.Add(field.Name, field.GetValue(field));
                }

                data = JsonConvert.SerializeObject(fieldDictionary);
            }
            else
            {
                foreach (var field in typeof(AppInfo_Linux).GetRuntimeFields())
                {
                    fieldDictionary.Add(field.Name, field.GetValue(field));
                }

                data = JsonConvert.SerializeObject(typeof(AppInfo_Linux).GetRuntimeFields());
            }

            await File.WriteAllTextAsync(AppInfo.ConfigFilePath, data, Encoding.UTF8);

            m_logInformation(_saveMessage);
        }

        public static Dictionary<string, List<(string, string)>> GetAllConfig()
        {
            Dictionary<string, List<(string, string)>> configProps = new Dictionary<string, List<(string, string)>>();

            foreach (var prop in typeof(AppInfo).GetFields())
            {
                if (!configProps.ContainsKey(nameof(AppInfo)))
                {
                    configProps.Add(nameof(AppInfo), new List<(string, string)>{
                        (prop.Name, prop?.GetValue(prop)?.ToString())}
                    );
                }
                else
                {
                    configProps[nameof(AppInfo)].Add((prop.Name, prop?.GetValue(prop)?.ToString()));
                }
            }

            if (AppInfo.IsWindows)
            {
                foreach (var prop in typeof(AppInfo_Windows).GetFields())
                {
                    if (!configProps.ContainsKey(nameof(AppInfo_Windows)))
                    {
                        configProps.Add(nameof(AppInfo_Windows), new List<(string, string)>() { (prop.Name, prop?.GetValue(prop)?.ToString()) });
                    }
                    else
                    {
                        configProps[nameof(AppInfo_Windows)].Add((prop.Name, prop?.GetValue(prop)?.ToString()));
                    }
                }
            }
            else
            {
                foreach (var prop in typeof(AppInfo_Linux).GetFields())
                {
                    if (!configProps.ContainsKey(nameof(AppInfo_Linux)))
                    {
                        configProps.Add(nameof(AppInfo_Linux), new List<(string, string)>{
                        (prop.Name, prop?.GetValue(prop)?.ToString())}
                        );
                    }
                    else
                    {
                        configProps[nameof(AppInfo_Linux)].Add((prop.Name, prop?.GetValue(prop)?.ToString()));
                    }
                }
            }

            return configProps;
        }

        private static bool IsConfigFileValid()
        {
            if (!File.Exists(AppInfo.ConfigFilePath))
            {
                return false;
            }
            if (!File.ReadAllText(AppInfo.ConfigFilePath).Any())
            {
                return false;
            }

            return true;
        }

        private static void RestoreConfigFile()
        {
            m_logInformation(_restoreMessageStart);

            if (!File.Exists(AppInfo.ConfigFilePath))
            {
                File.Create(AppInfo.ConfigFilePath).Close();
            }

            Save().GetAwaiter().GetResult();

            m_logInformation(_restoreMessageFinish);
        }

        private static bool ShouldUpdateConfigFile()
        {
            if (File.Exists(AppInfo.ConfigFilePath))
            {
                if (File.GetLastAccessTime(AppInfo.ConfigFilePath).AddDays(1) < DateTime.Now)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
