using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SwpfEditor.App.Services;

namespace SwpfEditor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private XDocument? _currentDocument;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    private TreeNodeViewModel? _rootNode;

    [ObservableProperty]
    private TreeNodeViewModel? _selectedNode;

    [ObservableProperty]
    private ObservableCollection<AttributeViewModel> _attributes = new();

    public ICommand NewCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ValidateCommand { get; }

    public MainViewModel()
    {
        NewCommand = new RelayCommand(CreateNew);
        OpenCommand = new RelayCommand(Open);
        SaveCommand = new RelayCommand(Save);
        ValidateCommand = new RelayCommand(Validate);
    }

    public void LoadDocument(XDocument document, string? filePath = null)
    {
        CurrentDocument = document;
        CurrentFilePath = filePath;
        
        if (document.Root != null)
        {
            RootNode = new TreeNodeViewModel(document.Root);
        }
        else
        {
            RootNode = null;
        }
    }

    public void SaveDocument(string filePath)
    {
        if (CurrentDocument == null) return;
        XmlFileService.SaveXmlFile(CurrentDocument, filePath);
        CurrentFilePath = filePath;
    }

    private void CreateNew()
    {
        var doc = new XDocument(new XElement("test"));
        LoadDocument(doc);
    }

    private void Open()
    {
        // This will be called from the view to open file dialog
        // keeping the actual dialog in the view for now
    }

    private void Save()
    {
        // This will be called from the view for save logic
        // keeping the actual file operations in view for now
    }

    private void Validate()
    {
        // Placeholder for validation
    }

    partial void OnSelectedNodeChanged(TreeNodeViewModel? value)
    {
        Attributes.Clear();
        if (value?.Element == null) return;

        foreach (var attr in value.Element.Attributes())
        {
            Attributes.Add(new AttributeViewModel(value.Element, attr, () => value.UpdateHeader()));
        }
    }
}

public partial class AttributeViewModel : ObservableObject
{
    private readonly XElement _owner;
    private readonly XName _name;
    private readonly Action? _onChanged;

    [ObservableProperty]
    private string _value;

    public string Name { get; }

    public AttributeViewModel(XElement owner, XAttribute attribute, Action? onChanged = null)
    {
        _owner = owner;
        _name = attribute.Name;
        _onChanged = onChanged;
        Name = attribute.Name.LocalName;
        _value = attribute.Value;
    }

    partial void OnValueChanged(string value)
    {
        var attr = _owner.Attribute(_name);
        if (attr != null && attr.Value != value)
        {
            attr.Value = value;
            _onChanged?.Invoke();
        }
    }
}