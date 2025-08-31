using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SwpfEditor.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private XDocument? _currentDocument;

    [ObservableProperty]
    private string? _currentFilePath;

    [ObservableProperty]
    private XElement? _selectedElement;

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

    private void CreateNew()
    {
        CurrentDocument = new XDocument(new XElement("test"));
        CurrentFilePath = null;
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

    partial void OnSelectedElementChanged(XElement? value)
    {
        Attributes.Clear();
        if (value == null) return;

        foreach (var attr in value.Attributes())
        {
            Attributes.Add(new AttributeViewModel(value, attr));
        }
    }
}

public partial class AttributeViewModel : ObservableObject
{
    private readonly XElement _owner;
    private readonly XName _name;

    [ObservableProperty]
    private string _value;

    public string Name { get; }

    public AttributeViewModel(XElement owner, XAttribute attribute)
    {
        _owner = owner;
        _name = attribute.Name;
        Name = attribute.Name.LocalName;
        _value = attribute.Value;
    }

    partial void OnValueChanged(string value)
    {
        var attr = _owner.Attribute(_name);
        if (attr != null && attr.Value != value)
        {
            attr.Value = value;
        }
    }
}