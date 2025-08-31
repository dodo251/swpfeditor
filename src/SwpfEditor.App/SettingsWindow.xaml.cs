using System;
using System.Globalization;
using System.Windows;
using SwpfEditor.App.Services;

namespace SwpfEditor.App
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly Action<AppSettings> _onSave;

        public SettingsWindow(AppSettings currentSettings, Action<AppSettings> onSave)
        {
            InitializeComponent();
            _settings = new AppSettings
            {
                StrongValidationBeforeSave = currentSettings.StrongValidationBeforeSave,
                IndentWidth = currentSettings.IndentWidth,
                AutoExpandDelayMs = currentSettings.AutoExpandDelayMs,
                MaxUndoSteps = currentSettings.MaxUndoSteps,
                AutoLoadSample = currentSettings.AutoLoadSample,
                ShowLineNumbers = currentSettings.ShowLineNumbers,
                MaxLogFileSizeMB = currentSettings.MaxLogFileSizeMB
            };
            _onSave = onSave;
            
            LoadSettingsToUI();
        }

        private void LoadSettingsToUI()
        {
            ChkStrongValidation.IsChecked = _settings.StrongValidationBeforeSave;
            ChkShowLineNumbers.IsChecked = _settings.ShowLineNumbers;
            ChkAutoLoadSample.IsChecked = _settings.AutoLoadSample;
            
            TxtIndentWidth.Text = _settings.IndentWidth.ToString();
            TxtAutoExpandDelay.Text = _settings.AutoExpandDelayMs.ToString();
            TxtMaxUndoSteps.Text = _settings.MaxUndoSteps.ToString();
            TxtMaxLogFileSize.Text = _settings.MaxLogFileSizeMB.ToString();
        }

        private bool SaveSettingsFromUI()
        {
            try
            {
                _settings.StrongValidationBeforeSave = ChkStrongValidation.IsChecked == true;
                _settings.ShowLineNumbers = ChkShowLineNumbers.IsChecked == true;
                _settings.AutoLoadSample = ChkAutoLoadSample.IsChecked == true;

                if (!int.TryParse(TxtIndentWidth.Text, out var indentWidth) || indentWidth < 1 || indentWidth > 10)
                {
                    MessageBox.Show("缩进宽度必须是 1-10 之间的数字", "设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtIndentWidth.Focus();
                    return false;
                }
                _settings.IndentWidth = indentWidth;

                if (!int.TryParse(TxtAutoExpandDelay.Text, out var autoExpandDelay) || autoExpandDelay < 100 || autoExpandDelay > 5000)
                {
                    MessageBox.Show("自动展开延时必须是 100-5000ms 之间的数字", "设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtAutoExpandDelay.Focus();
                    return false;
                }
                _settings.AutoExpandDelayMs = autoExpandDelay;

                if (!int.TryParse(TxtMaxUndoSteps.Text, out var maxUndoSteps) || maxUndoSteps < 10 || maxUndoSteps > 1000)
                {
                    MessageBox.Show("最大撤销步数必须是 10-1000 之间的数字", "设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtMaxUndoSteps.Focus();
                    return false;
                }
                _settings.MaxUndoSteps = maxUndoSteps;

                if (!int.TryParse(TxtMaxLogFileSize.Text, out var maxLogFileSize) || maxLogFileSize < 1 || maxLogFileSize > 100)
                {
                    MessageBox.Show("最大日志文件大小必须是 1-100MB 之间的数字", "设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtMaxLogFileSize.Focus();
                    return false;
                }
                _settings.MaxLogFileSizeMB = maxLogFileSize;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置时出错: {ex.Message}", "设置错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (SaveSettingsFromUI())
            {
                _onSave(_settings);
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}