using System;
using System.IO;
using System.Text.Json;

namespace SwpfEditor.App.Services
{
    /// <summary>
    /// Application settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Whether to perform strong validation before saving
        /// </summary>
        public bool StrongValidationBeforeSave { get; set; } = true;

        /// <summary>
        /// Indent width for XML formatting (spaces)
        /// </summary>
        public int IndentWidth { get; set; } = 2;

        /// <summary>
        /// Auto-expand delay for drag hover (milliseconds)
        /// </summary>
        public int AutoExpandDelayMs { get; set; } = 600;

        /// <summary>
        /// Maximum undo steps to maintain
        /// </summary>
        public int MaxUndoSteps { get; set; } = 50;

        /// <summary>
        /// Whether to auto-load sample file on startup
        /// </summary>
        public bool AutoLoadSample { get; set; } = true;

        /// <summary>
        /// Whether to show line numbers in validation messages
        /// </summary>
        public bool ShowLineNumbers { get; set; } = true;

        /// <summary>
        /// Maximum log file size in MB before rotation
        /// </summary>
        public int MaxLogFileSizeMB { get; set; } = 10;
    }

    /// <summary>
    /// Service for managing application settings
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Current settings
        /// </summary>
        AppSettings Settings { get; }

        /// <summary>
        /// Load settings from file
        /// </summary>
        void Load();

        /// <summary>
        /// Save settings to file
        /// </summary>
        void Save();

        /// <summary>
        /// Event fired when settings change
        /// </summary>
        event EventHandler<AppSettings>? SettingsChanged;
    }

    /// <summary>
    /// Implementation of settings service
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _settings = new();

        public AppSettings Settings => _settings;

        public event EventHandler<AppSettings>? SettingsChanged;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "SwpfEditor");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _settings = settings;
                    }
                }
            }
            catch (Exception)
            {
                // Use default settings if loading fails
                _settings = new AppSettings();
            }

            OnSettingsChanged();
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception)
            {
                // Silently fail - we don't want to crash the app over settings
            }
        }

        public void UpdateSettings(AppSettings newSettings)
        {
            _settings = newSettings ?? throw new ArgumentNullException(nameof(newSettings));
            Save();
            OnSettingsChanged();
        }

        private void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, _settings);
        }
    }
}