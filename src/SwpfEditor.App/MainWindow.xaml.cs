using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;
using SwpfEditor.App.ViewModels;
using SwpfEditor.Domain.Models;

namespace SwpfEditor.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            // Subscribe to validation errors changes
            _viewModel.ValidationErrors.CollectionChanged += (s, e) => RefreshValidationPanel();
            
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Try to load sample files on startup
            TryLoadSampleFiles();
        }

        private void BtnNew_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.NewFileCommand.Execute(null);
            RefreshTree();
        }

        private void BtnOpen_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.OpenFileCommand.Execute(null);
            RefreshTree();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveFileCommand.Execute(null);
        }

        private void BtnValidate_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ValidateFileCommand.Execute(null);
        }

        private void BtnExportRuntime_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ExportRuntimeXmlCommand.Execute(null);
        }

        private void ScriptTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _viewModel.SelectedTreeItem = e.NewValue;
            RefreshPropertiesPanel();
        }

        private void TryLoadSampleFiles()
        {
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var samplePath = Path.Combine(appDir, "samples", "test.xml");
                
                // Try to find samples relative to executable or in repository structure
                if (!File.Exists(samplePath))
                {
                    // Try going up directories to find samples (for development scenario)
                    var currentDir = new DirectoryInfo(appDir);
                    while (currentDir != null && currentDir.Parent != null)
                    {
                        var testSamplesPath = Path.Combine(currentDir.FullName, "samples", "test.xml");
                        if (File.Exists(testSamplesPath))
                        {
                            samplePath = testSamplesPath;
                            break;
                        }
                        currentDir = currentDir.Parent;
                    }
                }

                if (File.Exists(samplePath))
                {
                    LoadFile(samplePath);
                }
                else
                {
                    // Create a simple test if no samples found
                    _viewModel.NewFileCommand.Execute(null);
                }
            }
            catch
            {
                // If all else fails, create a new test
                _viewModel.NewFileCommand.Execute(null);
            }
        }

        private void LoadFile(string path)
        {
            try
            {
                using var sr = new StreamReader(path, Encoding.UTF8, true);
                var xml = XDocument.Load(sr, LoadOptions.SetLineInfo);
                
                if (xml.Root != null)
                {
                    // Use the ViewModel's load logic by simulating file open
                    _viewModel.CurrentFilePath = path;
                    var test = SwpfEditor.Infrastructure.Mapping.XmlTestMapper.FromXElement(xml.Root);
                    _viewModel.CurrentTest = test;
                    _viewModel.HasUnsavedChanges = false;
                    
                    RefreshTree();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}");
            }
        }

        private void RefreshTree()
        {
            ScriptTree.Items.Clear();
            PropertiesPanel.Children.Clear();
            
            if (_viewModel.CurrentTest == null) return;
            
            // For now, create a simple tree representation
            // TODO: Implement proper tree binding with templates
            var rootItem = new TreeViewItem
            {
                Header = $"Test: {_viewModel.CurrentTest.Alias ?? _viewModel.CurrentTest.Id}",
                Tag = _viewModel.CurrentTest,
                IsExpanded = true
            };

            // Add sections
            if (_viewModel.CurrentTest.Sections?.SectionList != null)
            {
                foreach (var section in _viewModel.CurrentTest.Sections.SectionList)
                {
                    var sectionItem = new TreeViewItem
                    {
                        Header = $"Section: {section.Alias ?? section.Id}",
                        Tag = section,
                        IsExpanded = true
                    };

                    // Add steps in section
                    foreach (var step in section.Steps)
                    {
                        var stepItem = new TreeViewItem
                        {
                            Header = ElementHeader(step),
                            Tag = step
                        };
                        sectionItem.Items.Add(stepItem);
                    }

                    rootItem.Items.Add(sectionItem);
                }
            }

            // Add direct steps
            if (_viewModel.CurrentTest.Steps?.StepList != null)
            {
                foreach (var step in _viewModel.CurrentTest.Steps.StepList)
                {
                    var stepItem = new TreeViewItem
                    {
                        Header = ElementHeader(step),
                        Tag = step
                    };
                    rootItem.Items.Add(stepItem);
                }
            }

            ScriptTree.Items.Add(rootItem);
        }

        private string ElementHeader(Step step)
        {
            var name = !string.IsNullOrEmpty(step.Alias) ? step.Alias : step.Id;
            var targetInfo = !string.IsNullOrEmpty(step.Target) ? $" -> {step.Target}" : "";
            return $"Step: {name}{targetInfo}";
        }

        private void RefreshPropertiesPanel()
        {
            PropertiesPanel.Children.Clear();
            
            if (_viewModel.SelectedTreeItem == null) return;

            // Create simple property editors based on selected item type
            if (_viewModel.SelectedTreeItem is Test test)
            {
                AddPropertyEditor("ID", test.Id, value => test.Id = value);
                AddPropertyEditor("Alias", test.Alias ?? "", value => test.Alias = string.IsNullOrEmpty(value) ? null : value);
                AddPropertyEditor("Description", test.Description ?? "", value => test.Description = string.IsNullOrEmpty(value) ? null : value);
            }
            else if (_viewModel.SelectedTreeItem is Section section)
            {
                AddPropertyEditor("ID", section.Id, value => section.Id = value);
                AddPropertyEditor("Alias", section.Alias ?? "", value => section.Alias = string.IsNullOrEmpty(value) ? null : value);
            }
            else if (_viewModel.SelectedTreeItem is Step step)
            {
                AddPropertyEditor("ID", step.Id, value => step.Id = value);
                AddPropertyEditor("Alias", step.Alias ?? "", value => step.Alias = string.IsNullOrEmpty(value) ? null : value);
                AddPropertyEditor("Target", step.Target ?? "", value => step.Target = string.IsNullOrEmpty(value) ? null : value);
                AddPropertyEditor("Command", step.Command ?? "", value => step.Command = string.IsNullOrEmpty(value) ? null : value);
                AddPropertyEditor("Timeout", step.Timeout?.ToString() ?? "", value => step.Timeout = int.TryParse(value, out var t) ? t : null);
                
                // Target Type combo
                AddTargetTypeEditor(step);
                AddHttpMethodEditor(step);
            }
        }

        private void AddPropertyEditor(string label, string value, Action<string> onChanged)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            
            var labelControl = new Label { Content = label, Width = 80 };
            var textBox = new TextBox { Text = value, Width = 200 };
            
            textBox.TextChanged += (s, e) => 
            {
                onChanged(textBox.Text);
                _viewModel.HasUnsavedChanges = true;
            };
            
            panel.Children.Add(labelControl);
            panel.Children.Add(textBox);
            PropertiesPanel.Children.Add(panel);
        }

        private void AddTargetTypeEditor(Step step)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            
            var label = new Label { Content = "Target Type", Width = 80 };
            var combo = new ComboBox { Width = 200 };
            
            foreach (var type in Enum.GetValues<TargetType>())
            {
                combo.Items.Add(type);
            }
            
            combo.SelectedItem = step.TargetType;
            combo.SelectionChanged += (s, e) => 
            {
                step.TargetType = (TargetType?)combo.SelectedItem;
                _viewModel.HasUnsavedChanges = true;
            };
            
            panel.Children.Add(label);
            panel.Children.Add(combo);
            PropertiesPanel.Children.Add(panel);
        }

        private void AddHttpMethodEditor(Step step)
        {
            if (step.TargetType != TargetType.Http) return;

            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            
            var label = new Label { Content = "Method", Width = 80 };
            var combo = new ComboBox { Width = 200 };
            
            foreach (var method in Enum.GetValues<HttpMethod>())
            {
                combo.Items.Add(method);
            }
            
            combo.SelectedItem = step.Method;
            combo.SelectionChanged += (s, e) => 
            {
                step.Method = (HttpMethod?)combo.SelectedItem;
                _viewModel.HasUnsavedChanges = true;
            };
            
            panel.Children.Add(label);
            panel.Children.Add(combo);
            PropertiesPanel.Children.Add(panel);
        }

        private void RefreshValidationPanel()
        {
            // Update validation display (will be improved in later stage)
            var errorCount = _viewModel.ValidationErrors.Count;
            Title = $"SMS Script Editor - {errorCount} validation issues";
        }
    }
}