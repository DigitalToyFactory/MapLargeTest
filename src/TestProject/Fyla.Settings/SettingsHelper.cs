using System.ComponentModel;
using System.Globalization;

namespace Fyla.Settings
{
    public static class SettingsHelper
    {
        public static string GetSetDefaultValue(this ISettingsManager manager, string key, string defaultValue)
        {
            var current = manager.Get(key);

            if (string.IsNullOrEmpty(current))
            {
                manager.Set(key, defaultValue);
                return defaultValue;
            }

            return current;
        }
    }
}