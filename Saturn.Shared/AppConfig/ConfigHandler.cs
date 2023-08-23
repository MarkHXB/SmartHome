using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace Saturn.Shared
{
    public class ConfigHandler
    {
        private const string _saveMessage = " @Config ConfigHandler saved.";
        private const string _loadMessage = " @Config ConfigHandler loaded.";
        private const string _restoreMessageStart = " @Config ConfigHandler restoring started.";
        private const string _restoreMessageFinish = " @Config ConfigHandler restoring finished.";
        private const string _buildMessage = " @Config ConfigHandler successfully built.";
        private static bool _initialized = false;

        private static Action<string>? m_logInformation;

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

            if (!IsConfigFileValid() || IsConfigFileChanged().GetAwaiter().GetResult())
            {
                RestoreConfigFile();
            }

            Load().GetAwaiter().GetResult();

            _initialized = true;

            m_logInformation(_buildMessage);
        }

        

        public static async Task Load()
        {
            _ = m_logInformation ?? throw new ArgumentNullException(nameof(m_logInformation));

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
            _ = m_logInformation ?? throw new ArgumentNullException(nameof(m_logInformation));

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

                data = JsonConvert.SerializeObject(fieldDictionary);
            }

            await File.WriteAllTextAsync(AppInfo.ConfigFilePath, data, Encoding.UTF8);

            m_logInformation(_saveMessage);
        }

        public static Dictionary<string, List<KeyValuePair<string, string>>> GetAllConfig()
        {
            Dictionary<string, List<KeyValuePair<string, string>>> configProps = new Dictionary<string, List<KeyValuePair<string, string>>>();

            foreach (var prop in typeof(AppInfo).GetFields())
            {
                string key = prop.Name;
                string value = prop?.GetValue(prop)?.ToString();

                if (!configProps.ContainsKey(nameof(AppInfo)))
                {
                    configProps.Add(nameof(AppInfo), new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>(key,value)
                    });
                }
                else
                {
                    configProps[nameof(AppInfo)].Add(new KeyValuePair<string, string>(key, value));
                }
            }

            if (AppInfo.IsWindows)
            {
                foreach (var prop in typeof(AppInfo_Windows).GetFields())
                {
                    string key = prop.Name;
                    string value = prop?.GetValue(prop)?.ToString();

                    if (!configProps.ContainsKey(nameof(AppInfo_Windows)))
                    {
                        configProps.Add(nameof(AppInfo_Windows), new List<KeyValuePair<string, string>>()
                        {
                        new KeyValuePair<string, string>(key,value)
                    });
                    }
                    else
                    {
                        configProps[nameof(AppInfo_Windows)].Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
            else
            {
                foreach (var prop in typeof(AppInfo_Linux).GetFields())
                {
                    string key = prop.Name;
                    string value = prop?.GetValue(prop)?.ToString();

                    if (!configProps.ContainsKey(nameof(AppInfo_Linux)))
                    {
                        configProps.Add(nameof(AppInfo_Linux), new List<KeyValuePair<string, string>>()
                        {
                        new KeyValuePair<string, string>(key,value)
                    });
                    }
                    else
                    {
                        configProps[nameof(AppInfo_Linux)].Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }

            return configProps;
        }

        public static void SetConfig(string propertyName, string type, string value)
        {
            if (!_initialized)
            {
                throw new Exception("You should initialize first the handler");
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            object _value = null;
            type = type.Trim().ToLower();

            if (type.Contains("int"))
            {
                int.TryParse(value, out int val);
                _value = val;
            }
            else if (type.Contains("bool"))
            {
                bool.TryParse(value, out bool val);
                _value = val;
            }
            else if (type.Contains("datetime"))
            {
                DateTime.TryParse(value, out DateTime val);
                _value = val;
            }

            if (_value is null)
            {
                throw new Exception("Unrecognized type of value");
            }

            if (AppInfo.IsWindows)
            {
                if (!typeof(AppInfo_Windows).GetFields().Any(field => field.Name.Equals(propertyName)))
                {
                    throw new KeyNotFoundException(propertyName);
                }

                foreach (var field in typeof(AppInfo_Windows).GetFields())
                {
                    if (field.Name.Equals(propertyName))
                    {
                        field.SetValue(field, _value);
                    }
                }
            }
            else
            {
                if (!typeof(AppInfo_Linux).GetFields().Any(field => field.Name.Equals(propertyName)))
                {
                    throw new KeyNotFoundException(propertyName);
                }

                foreach (var field in typeof(AppInfo_Linux).GetFields())
                {
                    if (field.Name.Equals(propertyName))
                    {
                        field.SetValue(field, _value);
                    }
                }
            }

            ConfigHandler.Save().GetAwaiter().GetResult();
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

        private static async Task<bool> IsConfigFileChanged()
        {
            _ = m_logInformation ?? throw new ArgumentNullException(nameof(m_logInformation));

            string raw = await File.ReadAllTextAsync(AppInfo.ConfigFilePath);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);

            // FOR WINDOWS
            if (AppInfo.IsWindows)
            { 
                var current = typeof(AppInfo_Windows).GetRuntimeFields();

                foreach (var old in data)
                {
                    if(!current.Any(c => c.Name.Contains(old.Key)))
                    {
                        return true;
                    }
                }
            }
            else
            {
                var current = typeof(AppInfo_Linux).GetRuntimeFields();

                foreach (var old in data)
                {
                    if (!current.Any(c => c.Name.Contains(old.Key)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
