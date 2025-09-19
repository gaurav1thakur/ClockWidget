using System.IO;
using System.Text.Json;
using System.Xml;

namespace ClockWidget
{
    public class Config
    {
        private static string ConfigPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "ClockWidget", "config.json");

        public string Theme { get; set; } = "Dark";
        public bool ClickThroughEnabled { get; set; } = false;
        public double ClockOpacity { get; set; } = 3.0;
        public string ClockSize { get; set; } = "Large";
        public int LastFocusMinutes { get; set; } = 25;
        public bool IsOverlayMode { get; set; } = true; // default to overlay


        private static Config _current;
        public static Config Current
        {
            get
            {
                if (_current == null)
                {
                    try
                    {
                        if (File.Exists(ConfigPath))
                        {
                            string json = File.ReadAllText(ConfigPath);
                            _current = JsonSerializer.Deserialize<Config>(json);
                        }
                    }
                    catch { }
                    if (_current == null) _current = new Config();
                }
                return _current;
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }


        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }


        public static Config Load(string path)
        {
            if (!File.Exists(path)) return new Config();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }


    }
}

