using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SwpfEditor.App.Services;

namespace SwpfEditor.App.ViewModels
{
    /// <summary>
    /// ViewModel for template display in UI
    /// </summary>
    public class TemplateViewModel : INotifyPropertyChanged
    {
        private readonly ElementTemplate _template;

        public TemplateViewModel(ElementTemplate template)
        {
            _template = template;
        }

        public string Name => _template.Name;
        public string DisplayName => _template.DisplayName;
        public string Description => _template.Description;
        public ElementTemplate Template => _template;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// ViewModel for the templates panel
    /// </summary>
    public class TemplatesViewModel : INotifyPropertyChanged
    {
        private readonly ITemplateService _templateService;
        private readonly ObservableCollection<TemplateViewModel> _templates = new();
        private string _selectedParentElement = string.Empty;

        public TemplatesViewModel(ITemplateService templateService)
        {
            _templateService = templateService;
            LoadTemplates();
        }

        public ObservableCollection<TemplateViewModel> Templates => _templates;

        public string SelectedParentElement
        {
            get => _selectedParentElement;
            set
            {
                if (_selectedParentElement != value)
                {
                    _selectedParentElement = value;
                    OnPropertyChanged();
                    FilterTemplates();
                }
            }
        }

        public void LoadTemplates()
        {
            _templates.Clear();
            
            if (!_templateService.IsLoaded)
            {
                // Try to load default templates
                _templateService.LoadTemplates("Template.xml");
            }

            foreach (var template in _templateService.GetTemplates())
            {
                _templates.Add(new TemplateViewModel(template));
            }
        }

        public void FilterTemplates()
        {
            // In a more sophisticated implementation, this would filter based on selected parent
            // For now, we show all templates
            LoadTemplates();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}