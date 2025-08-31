using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SwpfEditor.App.Services
{
    /// <summary>
    /// Log entry level
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Log entry
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    /// <summary>
    /// Service for application logging
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Log an info message
        /// </summary>
        void LogInfo(string message, string? details = null);

        /// <summary>
        /// Log a warning message
        /// </summary>
        void LogWarning(string message, string? details = null);

        /// <summary>
        /// Log an error message
        /// </summary>
        void LogError(string message, string? details = null);

        /// <summary>
        /// Get recent log entries
        /// </summary>
        IEnumerable<LogEntry> GetRecentEntries(int count = 100);

        /// <summary>
        /// Clear all log entries
        /// </summary>
        void Clear();

        /// <summary>
        /// Event fired when new log entry is added
        /// </summary>
        event EventHandler<LogEntry>? LogEntryAdded;
    }

    /// <summary>
    /// Implementation of logging service with file and memory storage
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private readonly List<LogEntry> _entries = new();
        private readonly object _lock = new();
        private readonly int _maxMemoryEntries = 1000;

        public event EventHandler<LogEntry>? LogEntryAdded;

        public LoggingService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "SwpfEditor");
            Directory.CreateDirectory(appFolder);
            _logFilePath = Path.Combine(appFolder, "app.log");
        }

        public void LogInfo(string message, string? details = null)
        {
            AddLogEntry(LogLevel.Info, message, details);
        }

        public void LogWarning(string message, string? details = null)
        {
            AddLogEntry(LogLevel.Warning, message, details);
        }

        public void LogError(string message, string? details = null)
        {
            AddLogEntry(LogLevel.Error, message, details);
        }

        public IEnumerable<LogEntry> GetRecentEntries(int count = 100)
        {
            lock (_lock)
            {
                return _entries.TakeLast(count).ToList();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
            }
        }

        private void AddLogEntry(LogLevel level, string message, string? details)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Details = details
            };

            lock (_lock)
            {
                _entries.Add(entry);
                
                // Keep memory usage bounded
                if (_entries.Count > _maxMemoryEntries)
                {
                    _entries.RemoveAt(0);
                }
            }

            // Write to file asynchronously
            WriteToFileAsync(entry);

            // Notify listeners
            LogEntryAdded?.Invoke(this, entry);
        }

        private void WriteToFileAsync(LogEntry entry)
        {
            try
            {
                var logLine = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}";
                if (!string.IsNullOrEmpty(entry.Details))
                {
                    logLine += $" | {entry.Details}";
                }

                // Simple file appending - in production might want more sophisticated rotation
                File.AppendAllLines(_logFilePath, new[] { logLine });
            }
            catch
            {
                // Don't let logging failures crash the app
            }
        }
    }
}