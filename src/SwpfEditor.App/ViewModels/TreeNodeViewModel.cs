using System.Collections.ObjectModel;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SwpfEditor.App.Services;

namespace SwpfEditor.App.ViewModels;

public partial class TreeNodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _header = string.Empty;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isSelected;

    public XElement Element { get; }
    public ObservableCollection<TreeNodeViewModel> Children { get; } = new();

    public TreeNodeViewModel(XElement element)
    {
        Element = element;
        Header = XmlFileService.CreateElementHeader(element);
        
        foreach (var child in element.Elements())
        {
            Children.Add(new TreeNodeViewModel(child));
        }
    }

    public void UpdateHeader()
    {
        Header = XmlFileService.CreateElementHeader(Element);
    }
}