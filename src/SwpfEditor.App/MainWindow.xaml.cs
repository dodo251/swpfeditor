using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;
using SwpfEditor.App.Models;
using SwpfEditor.App.Services;
using SwpfEditor.App.Validation;
using SwpfEditor.App.ViewModels;

namespace SwpfEditor.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XDocument? _currentDoc;
        private string? _currentPath;
        
        // Services
        private readonly IUndoRedoService _undoRedoService;
        private readonly IValidationService _validationService;
        private readonly ITemplateService _templateService;
        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _loggingService;
        private readonly XmlDragDropHandler _dragDropHandler;
        private readonly TemplatesViewModel _templatesViewModel;
        
        // UI collections
        private readonly ObservableCollection<TemplateViewModel> _templates = new();
        private readonly ObservableCollection<LogEntry> _logEntries = new();
        
        // For suppressing change events during programmatic updates
        private bool _suppressChangeEvents = false;

        public ObservableCollection<TemplateViewModel> Templates => _templates;
        public ObservableCollection<LogEntry> LogEntries => _logEntries;
        public XmlDragDropHandler DragDropHandler => _dragDropHandler;

        public MainWindow()
        {
            // Initialize services
            _undoRedoService = new UndoRedoService();
            _validationService = new ValidationService();
            _templateService = new TemplateService();
            _settingsService = new SettingsService();
            _loggingService = new LoggingService();
            
            // Initialize drag-drop handler
            _dragDropHandler = new XmlDragDropHandler(
                _validationService,
                _templateService,
                _undoRedoService,
                _loggingService,
                FindTreeViewItemForElement,
                RefreshTree
            );
            
            // Initialize view models
            _templatesViewModel = new TemplatesViewModel(_templateService);
            
            InitializeComponent();
            
            // Set data context and initialize UI
            DataContext = this;
            
            // Wire up event handlers
            _undoRedoService.StateChanged += UndoRedoService_StateChanged;
            _loggingService.LogEntryAdded += LoggingService_LogEntryAdded;
            _settingsService.SettingsChanged += SettingsService_SettingsChanged;
            
            // Load settings and templates
            _settingsService.Load();
            _templateService.LoadTemplates("Template.xml");
            LoadTemplatesIntoUI();
            
            _loggingService.LogInfo("应用程序启动");
            
            // Auto load sample if exists and enabled
            if (_settingsService.Settings.AutoLoadSample)
            {
                var sample = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "test.xml");
                if (File.Exists(sample))
                {
                    LoadFile(sample);
                }
            }
        }

        private void BtnNew_OnClick(object sender, RoutedEventArgs e)
        {
            _currentDoc = new XDocument(new XElement("test"));
            _currentPath = null;
            RefreshTree();
        }

        private void BtnOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "XML (*.xml)|*.xml" };
            if (dlg.ShowDialog() == true)
            {
                LoadFile(dlg.FileName);
            }
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentDoc == null) return;
            
            // Perform validation if enabled
            if (_settingsService.Settings.StrongValidationBeforeSave)
            {
                // TODO: Add validation logic
                _loggingService.LogInfo("强验证已启用（待实现）");
            }
            
            if (string.IsNullOrEmpty(_currentPath))
            {
                var dlg = new SaveFileDialog { Filter = "XML (*.xml)|*.xml", FileName = "test.xml" };
                if (dlg.ShowDialog() != true) return;
                _currentPath = dlg.FileName;
            }
            
            try
            {
                var settings = new UTF8Encoding(false);
                using var fs = new StreamWriter(_currentPath!, false, settings);
                // Preserve attribute order by writing manually (LINQ to XML preserves by insertion order we keep) with SaveOptions.DisableFormatting then simple pretty print.
                _currentDoc.Save(fs, SaveOptions.None);
                
                _loggingService.LogInfo($"文件已保存: {_currentPath}");
                StatusText.Text = "已保存";
            }
            catch (Exception ex)
            {
                _loggingService.LogError("保存文件失败", ex.Message);
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUndo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_undoRedoService.CanUndo)
            {
                _undoRedoService.Undo();
                RefreshTree();
                _loggingService.LogInfo("撤销操作");
            }
        }

        private void BtnRedo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_undoRedoService.CanRedo)
            {
                _undoRedoService.Redo();
                RefreshTree();
                _loggingService.LogInfo("重做操作");
            }
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: Open settings dialog
            MessageBox.Show("设置对话框（待实现）", "设置", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnValidate_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentDoc?.Root == null)
            {
                MessageBox.Show("没有可验证的文档", "验证", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // TODO: Implement full validation logic
            _loggingService.LogInfo("开始验证...");
            
            // Basic structure validation
            int issueCount = 0;
            ValidateElement(_currentDoc.Root, ref issueCount);
            
            if (issueCount == 0)
            {
                _loggingService.LogInfo("验证通过");
                ValidationStatus.Text = "验证通过";
                MessageBox.Show("验证通过", "验证", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _loggingService.LogWarning($"发现 {issueCount} 个问题");
                ValidationStatus.Text = $"{issueCount} 个问题";
                MessageBox.Show($"发现 {issueCount} 个验证问题，请查看日志", "验证", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ValidateElement(XElement element, ref int issueCount)
        {
            // Validate children are allowed
            foreach (var child in element.Elements())
            {
                if (!_validationService.IsValidChild(element.Name.LocalName, child.Name.LocalName))
                {
                    var message = _validationService.GetValidationMessage(element.Name.LocalName, child.Name.LocalName);
                    _loggingService.LogError("验证错误", message);
                    issueCount++;
                }
                
                // Recursively validate children
                ValidateElement(child, ref issueCount);
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                using var sr = new StreamReader(path, Encoding.UTF8, true);
                var xml = XDocument.Load(sr, LoadOptions.SetLineInfo);
                _currentDoc = xml;
                _currentPath = path;
                _undoRedoService.Clear(); // Clear undo history for new file
                RefreshTree();
                
                _loggingService.LogInfo($"文件已加载: {path}");
                StatusText.Text = $"已加载: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                _loggingService.LogError("加载文件失败", ex.Message);
                MessageBox.Show($"加载失败: {ex.Message}");
            }
        }

        private void RefreshTree()
        {
            ScriptTree.Items.Clear();
            PropertiesPanel.Children.Clear();
            if (_currentDoc?.Root == null) return;
            ScriptTree.Items.Add(CreateTreeItem(_currentDoc.Root));
        }

        private TreeViewItem CreateTreeItem(XElement element)
        {
            var tvi = new TreeViewItem { Header = ElementHeader(element), Tag = element, IsExpanded = true };
            foreach (var child in element.Elements())
            {
                tvi.Items.Add(CreateTreeItem(child));
            }
            return tvi;
        }

        private string ElementHeader(XElement el)
        {
            var id = el.Attribute("id")?.Value;
            var alias = el.Attribute("alias")?.Value;
            if (!string.IsNullOrWhiteSpace(alias)) return $"{el.Name.LocalName} ({alias})";
            if (!string.IsNullOrWhiteSpace(id)) return $"{el.Name.LocalName} ({id})";
            return el.Name.LocalName;
        }

        private void ScriptTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PropertiesPanel.Children.Clear();
            if (ScriptTree.SelectedItem is not TreeViewItem tvi || tvi.Tag is not XElement el) return;
            AddLabel($"元素: <{el.Name.LocalName}>");
            foreach (var attr in el.Attributes())
            {
                AddAttrEditor(el, attr);
            }
        }

        private void AddLabel(string text)
        {
            PropertiesPanel.Children.Add(new TextBlock { Text = text, Margin = new Thickness(0, 8, 0, 4), FontWeight = FontWeights.Bold });
        }

        private void AddAttrEditor(XElement owner, XAttribute attr)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            panel.Children.Add(new TextBlock { Text = attr.Name.LocalName, Width = 120, VerticalAlignment = VerticalAlignment.Center });
            var tb = new TextBox { Text = attr.Value, Width = 200, Tag = (owner, attr.Name) };
            tb.TextChanged += AttrTextChanged;
            panel.Children.Add(tb);
            PropertiesPanel.Children.Add(panel);
        }

        private void AttrTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressChangeEvents) return;
            
            if (sender is TextBox tb && tb.Tag is ValueTuple<XElement, XName> tag)
            {
                var (owner, name) = tag;
                var xa = owner.Attribute(name);
                if (xa != null && xa.Value != tb.Text)
                {
                    // Use undo/redo system for attribute changes
                    var command = new ChangeAttributeCommand(owner, name, tb.Text);
                    
                    // Temporarily suppress events to avoid recursion
                    _suppressChangeEvents = true;
                    try
                    {
                        _undoRedoService.ExecuteCommand(command);
                        
                        // Update header of tree node
                        if (ScriptTree.SelectedItem is TreeViewItem sel && sel.Tag == owner)
                        {
                            sel.Header = ElementHeader(owner);
                        }
                    }
                    finally
                    {
                        _suppressChangeEvents = false;
                    }
                }
            }
        }
        
        #region Event Handlers
        
        private void UndoRedoService_StateChanged(object? sender, EventArgs e)
        {
            // Update undo/redo button states
            Dispatcher.Invoke(() =>
            {
                BtnUndo.IsEnabled = _undoRedoService.CanUndo;
                BtnRedo.IsEnabled = _undoRedoService.CanRedo;
            });
        }
        
        private void LoggingService_LogEntryAdded(object? sender, LogEntry e)
        {
            // Add to UI collection (thread-safe)
            Dispatcher.Invoke(() =>
            {
                _logEntries.Add(e);
                
                // Keep UI list bounded
                while (_logEntries.Count > 100)
                {
                    _logEntries.RemoveAt(0);
                }
                
                // Auto-scroll to latest entry
                if (LogListBox.Items.Count > 0)
                {
                    LogListBox.ScrollIntoView(LogListBox.Items[^1]);
                }
            });
        }
        
        private void SettingsService_SettingsChanged(object? sender, AppSettings e)
        {
            // Update undo/redo service max steps
            _undoRedoService.MaxUndoSteps = e.MaxUndoSteps;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadTemplatesIntoUI()
        {
            _templates.Clear();
            foreach (var template in _templateService.GetTemplates())
            {
                _templates.Add(new TemplateViewModel(template));
            }
        }
        
        private TreeViewItem? FindTreeViewItemForElement(XElement element)
        {
            return FindTreeViewItemRecursive(ScriptTree.Items, element);
        }
        
        private TreeViewItem? FindTreeViewItemRecursive(ItemCollection items, XElement element)
        {
            foreach (TreeViewItem item in items)
            {
                if (ReferenceEquals(item.Tag, element))
                    return item;
                    
                var found = FindTreeViewItemRecursive(item.Items, element);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        #endregion
    }
}