using Newtonsoft.Json;
using System.Text.Json;
using System.Timers;

using Timer = System.Timers.Timer;

namespace Fyla.Settings
{
    public class SettingsManager : MemorySettingsManager
    {
        private Timer _fileSaveTimer = new Timer(1000);

        public override void Initialize()
        {
            base.Initialize();
            _fileSaveTimer.Elapsed += OnFileSaveTimerElapsed;
            ScheduleSaving();
        }

        private void OnFileSaveTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _fileSaveTimer.Stop();

            try
            {
                var values = string.Join(",\n", _currentValues.OrderBy(_ => _.Key).Select(_ => $"  \"{_.Key}\": {Encode(_.Value)}"));
                File.WriteAllText(_settingsFilePath, $"{{\n{values}\n}}");
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private string Encode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "null";
            }

            var trimmed = value.TrimStart();
            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            {
                return value;
            }

            return JsonConvert.SerializeObject(value);
        }


        public override void Set(string key, string value)
        {
            _currentValues.AddOrUpdate(key, s =>
            {
                ScheduleSaving();
                return value;
            }, 
            (s, o) =>
            {
                if (o != value)
                {
                    ScheduleSaving();
                }

                return value;
            });
        }

        protected virtual void ScheduleSaving()
        {
            _fileSaveTimer.Stop();
            _fileSaveTimer.Start();
        }
    }
}
