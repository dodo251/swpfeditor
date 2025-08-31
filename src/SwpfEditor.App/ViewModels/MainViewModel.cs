using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Services;
using SwpfEditor.Infrastructure.Mapping;

namespace SwpfEditor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IConnectionResolver _connectionResolver;
    private readonly IPlaceholderResolver _placeholderResolver;
    private readonly IXmlValidator _validator;

    [ObservableProperty]
    private Test? _currentTest;

    [ObservableProperty]
    private TestConfiguration? _currentTestConfiguration;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private object? _selectedTreeItem;

    [ObservableProperty]
    private ObservableCollection<ValidationError> _validationErrors = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public MainViewModel(
        IConnectionResolver connectionResolver,
        IPlaceholderResolver placeholderResolver,
        IXmlValidator validator)
    {
        _connectionResolver = connectionResolver;
        _placeholderResolver = placeholderResolver;
        _validator = validator;
    }

    [RelayCommand]
    private void NewFile()
    {
        if (HasUnsavedChanges)
        {
            var result = MessageBox.Show("保存当前文件？", "未保存的更改", 
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Cancel)
                return;
            
            if (result == MessageBoxResult.Yes)
                SaveFile();
        }

        CurrentTest = new Test { Id = "NewTest", Alias = "New Test" };
        CurrentFilePath = null;
        HasUnsavedChanges = false;
        StatusMessage = "新建文件";
        ValidateCurrentTest();
    }

    [RelayCommand]
    private void OpenFile()
    {
        if (HasUnsavedChanges)
        {
            var result = MessageBox.Show("保存当前文件？", "未保存的更改", 
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Cancel)
                return;
            
            if (result == MessageBoxResult.Yes)
                SaveFile();
        }

        var dialog = new OpenFileDialog
        {
            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            Title = "打开测试脚本"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadFile(dialog.FileName);
        }
    }

    [RelayCommand]
    private void SaveFile()
    {
        if (CurrentTest == null) return;

        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            SaveFileAs();
            return;
        }

        try
        {
            var xmlElement = XmlTestMapper.ToXElement(CurrentTest);
            var document = new XDocument(xmlElement);
            
            // Save with UTF-8 encoding and 2-space indentation
            var settings = new UTF8Encoding(false);
            using var fs = new StreamWriter(CurrentFilePath, false, settings);
            document.Save(fs, SaveOptions.None);
            
            HasUnsavedChanges = false;
            StatusMessage = $"已保存: {Path.GetFileName(CurrentFilePath)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SaveFileAs()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "XML Files (*.xml)|*.xml",
            Title = "另存为",
            FileName = "test.xml"
        };

        if (dialog.ShowDialog() == true)
        {
            CurrentFilePath = dialog.FileName;
            SaveFile();
        }
    }

    [RelayCommand]
    private void ValidateFile()
    {
        ValidateCurrentTest();
    }

    [RelayCommand]
    private void ExportRuntimeXml()
    {
        if (CurrentTest == null)
        {
            MessageBox.Show("没有测试脚本可导出", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Create runtime version with resolved placeholders and connections
            var runtimeTest = CreateRuntimeTest();
            
            // Ensure dist/runtime directory exists
            var outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dist", "runtime");
            Directory.CreateDirectory(outputDir);
            
            var outputPath = Path.Combine(outputDir, "test.exec.xml");
            
            var xmlElement = XmlTestMapper.ToXElement(runtimeTest);
            var document = new XDocument(xmlElement);
            
            // Save with UTF-8 encoding and 2-space indentation
            var settings = new UTF8Encoding(false);
            using var fs = new StreamWriter(outputPath, false, settings);
            document.Save(fs, SaveOptions.None);
            
            StatusMessage = $"运行时XML已导出: {outputPath}";
            MessageBox.Show($"运行时XML已导出至:\n{outputPath}", "导出成功", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadFile(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, true);
            var document = XDocument.Load(reader, LoadOptions.SetLineInfo);
            
            if (document.Root == null)
            {
                MessageBox.Show("无效的XML文件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CurrentTest = XmlTestMapper.FromXElement(document.Root);
            CurrentFilePath = filePath;
            HasUnsavedChanges = false;
            StatusMessage = $"已加载: {Path.GetFileName(filePath)}";
            
            // Try to load corresponding TestConfiguration.xml
            LoadTestConfiguration(filePath);
            
            ValidateCurrentTest();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTestConfiguration(string testFilePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(testFilePath);
            var configPath = Path.Combine(directory ?? "", "TestConfiguration.xml");
            
            if (File.Exists(configPath))
            {
                using var reader = new StreamReader(configPath, Encoding.UTF8, true);
                var document = XDocument.Load(reader);
                
                if (document.Root != null)
                {
                    CurrentTestConfiguration = XmlTestConfigurationMapper.FromXElement(document.Root);
                    StatusMessage += $" + {Path.GetFileName(configPath)}";
                }
            }
        }
        catch (Exception ex)
        {
            // TestConfiguration is optional, so just log but don't show error to user
            StatusMessage += " (TestConfiguration加载失败)";
        }
    }

    private void ValidateCurrentTest()
    {
        ValidationErrors.Clear();
        
        if (CurrentTest == null) return;

        try
        {
            // Convert to XML for XSD validation
            var xmlElement = XmlTestMapper.ToXElement(CurrentTest);
            var document = new XDocument(xmlElement);
            var xmlContent = document.ToString();
            
            // Comprehensive validation
            var errors = _validator.ValidateComprehensive(xmlContent, CurrentTest, CurrentTestConfiguration);
            
            foreach (var error in errors)
            {
                ValidationErrors.Add(error);
            }
            
            StatusMessage = $"验证完成: {errors.Count} 个问题";
        }
        catch (Exception ex)
        {
            ValidationErrors.Add(new ValidationError
            {
                Message = $"验证失败: {ex.Message}",
                Severity = ValidationSeverity.Error
            });
        }
    }

    private Test CreateRuntimeTest()
    {
        if (CurrentTest == null)
            throw new InvalidOperationException("No test to export");

        // Create a deep copy of the test
        var xmlElement = XmlTestMapper.ToXElement(CurrentTest);
        var runtimeTest = XmlTestMapper.FromXElement(xmlElement);
        
        // Auto-generate sessions if empty and we have test configuration
        if ((runtimeTest.Sessions == null || runtimeTest.Sessions.SessionList.Count == 0) && 
            CurrentTestConfiguration != null)
        {
            runtimeTest.Sessions = _connectionResolver.GenerateSessionMappings(runtimeTest, CurrentTestConfiguration);
        }

        // Resolve placeholders if we have test configuration
        if (CurrentTestConfiguration != null)
        {
            var context = PlaceholderContext.FromTestConfiguration(CurrentTestConfiguration);
            ResolvePlaceholdersInTest(runtimeTest, context);
        }

        return runtimeTest;
    }

    private void ResolvePlaceholdersInTest(Test test, PlaceholderContext context)
    {
        // Resolve placeholders in steps
        if (test.Steps?.StepList != null)
        {
            foreach (var step in test.Steps.StepList)
            {
                ResolvePlaceholdersInStep(step, context);
            }
        }

        // Resolve placeholders in sections
        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                foreach (var step in section.Steps)
                {
                    ResolvePlaceholdersInStep(step, context);
                }
            }
        }
    }

    private void ResolvePlaceholdersInStep(Step step, PlaceholderContext context)
    {
        if (!string.IsNullOrEmpty(step.Command))
        {
            step.Command = _placeholderResolver.ResolvePlaceholders(step.Command, context);
        }
        
        if (!string.IsNullOrEmpty(step.Target))
        {
            step.Target = _placeholderResolver.ResolvePlaceholders(step.Target, context);
        }

        // Resolve placeholders in parameters and headers
        if (step.Params?.ParamList != null)
        {
            foreach (var param in step.Params.ParamList)
            {
                param.Value = _placeholderResolver.ResolvePlaceholders(param.Value, context);
            }
        }

        if (step.Headers?.HeaderList != null)
        {
            foreach (var header in step.Headers.HeaderList)
            {
                header.Value = _placeholderResolver.ResolvePlaceholders(header.Value, context);
            }
        }
    }

    partial void OnCurrentTestChanged(Test? value)
    {
        HasUnsavedChanges = true;
        ValidateCurrentTest();
    }

    partial void OnSelectedTreeItemChanged(object? value)
    {
        // Trigger property panel updates when tree selection changes
    }
}