using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Reflection;

namespace Fyla.Settings
{
    public class MemorySettingsManager : ISettingsManager
    {
        protected bool _initialized;
        protected ConcurrentDictionary<string, string> _currentValues = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        protected readonly string _settingsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty, "settings.json");

        public virtual void Initialize()
        {
            lock (this)
            {
                if (_initialized)
                {
                    return;
                }

                try
                {
                    _currentValues = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    
                    if (File.Exists(_settingsFilePath))
                    {
                        var json = File.ReadAllText(_settingsFilePath);
                        var obj = JsonConvert.DeserializeObject<JObject>(json);

                        if (obj != null)
                        {
                            foreach (var prop in obj.Properties())
                            {
                                _currentValues[prop.Name] = prop.Value.ToString(Formatting.None);
                            }
                        }
                    }
                }
                catch
                {
                    _currentValues = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                _initialized = true;
            }
        }

        public virtual string Get(string key) => _currentValues.TryGetValue(key, out var value) ? value : null;

        public virtual void Set(string key, string value)
        {
            _currentValues.AddOrUpdate(key, s => value, (s, o) => value);
        }
    }
}