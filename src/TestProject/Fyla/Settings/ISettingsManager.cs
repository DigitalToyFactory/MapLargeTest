namespace Firefly.Settings
{
    public interface ISettingsManager
    {
        void Initialize();
        string Get(string key);
        void Set(string key, string value);
    }
}