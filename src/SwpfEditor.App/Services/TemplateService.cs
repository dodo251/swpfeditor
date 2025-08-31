using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SwpfEditor.App.Services
{
    /// <summary>
    /// Template definition for drag-and-drop
    /// </summary>
    public class ElementTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public XElement Template { get; set; } = new("placeholder");
        public List<string> AllowedParents { get; set; } = new();
    }

    /// <summary>
    /// Service for managing templates and creating default elements
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// Load templates from Template.xml
        /// </summary>
        void LoadTemplates(string templateFilePath);

        /// <summary>
        /// Get all available templates
        /// </summary>
        IEnumerable<ElementTemplate> GetTemplates();

        /// <summary>
        /// Get templates that can be added to the specified parent
        /// </summary>
        IEnumerable<ElementTemplate> GetTemplatesForParent(string parentElementName);

        /// <summary>
        /// Create a new element from template with default values
        /// </summary>
        XElement CreateElementFromTemplate(string templateName);

        /// <summary>
        /// Check if template is loaded
        /// </summary>
        bool IsLoaded { get; }
    }

    /// <summary>
    /// Implementation of template service
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly List<ElementTemplate> _templates = new();
        private bool _isLoaded = false;

        public bool IsLoaded => _isLoaded;

        public void LoadTemplates(string templateFilePath)
        {
            _templates.Clear();
            _isLoaded = false;

            if (!File.Exists(templateFilePath))
            {
                // Create default templates based on common patterns
                CreateDefaultTemplates();
                _isLoaded = true;
                return;
            }

            try
            {
                var doc = XDocument.Load(templateFilePath);
                if (doc.Root == null) return;

                foreach (var templateElement in doc.Root.Elements("template"))
                {
                    var template = ParseTemplate(templateElement);
                    if (template != null)
                    {
                        _templates.Add(template);
                    }
                }

                _isLoaded = true;
            }
            catch (Exception)
            {
                // Fall back to default templates if loading fails
                CreateDefaultTemplates();
                _isLoaded = true;
            }
        }

        public IEnumerable<ElementTemplate> GetTemplates()
        {
            return _templates.AsReadOnly();
        }

        public IEnumerable<ElementTemplate> GetTemplatesForParent(string parentElementName)
        {
            return _templates.Where(t => 
                t.AllowedParents.Count == 0 || 
                t.AllowedParents.Contains(parentElementName, StringComparer.OrdinalIgnoreCase));
        }

        public XElement CreateElementFromTemplate(string templateName)
        {
            var template = _templates.FirstOrDefault(t => 
                string.Equals(t.Name, templateName, StringComparison.OrdinalIgnoreCase));
            
            if (template == null)
            {
                throw new ArgumentException($"Template '{templateName}' not found");
            }

            // Create a deep copy of the template
            var newElement = new XElement(template.Template);
            
            // Generate unique IDs if needed
            GenerateUniqueIds(newElement);
            
            return newElement;
        }

        private ElementTemplate? ParseTemplate(XElement templateElement)
        {
            var name = templateElement.Attribute("name")?.Value;
            if (string.IsNullOrEmpty(name)) return null;

            var displayName = templateElement.Attribute("displayName")?.Value ?? name;
            var description = templateElement.Attribute("description")?.Value ?? "";
            
            var allowedParents = templateElement.Attribute("allowedParents")?.Value
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ?.Select(p => p.Trim())
                ?.ToList() ?? new List<string>();

            var elementTemplate = templateElement.Element("element");
            if (elementTemplate == null) return null;

            return new ElementTemplate
            {
                Name = name,
                DisplayName = displayName,
                Description = description,
                Template = new XElement(elementTemplate),
                AllowedParents = allowedParents
            };
        }

        private void CreateDefaultTemplates()
        {
            // Step template
            _templates.Add(new ElementTemplate
            {
                Name = "step",
                DisplayName = "Test Step",
                Description = "A basic test step",
                Template = new XElement("step",
                    new XAttribute("id", "step_placeholder"),
                    new XAttribute("alias", "New Step"),
                    new XAttribute("targetType", "ssh"),
                    new XAttribute("target", "DUT_SSH"),
                    new XAttribute("timeout", "30")),
                AllowedParents = new List<string> { "steps" }
            });

            // Section template
            _templates.Add(new ElementTemplate
            {
                Name = "section",
                DisplayName = "Section",
                Description = "A test section container",
                Template = new XElement("section",
                    new XAttribute("id", "section_placeholder"),
                    new XAttribute("alias", "New Section")),
                AllowedParents = new List<string> { "sections" }
            });

            // Extract template
            _templates.Add(new ElementTemplate
            {
                Name = "extract",
                DisplayName = "Extract",
                Description = "Data extraction from command output",
                Template = new XElement("extract",
                    new XAttribute("name", "extracted_value"),
                    new XAttribute("pattern", ".*")),
                AllowedParents = new List<string> { "extracts" }
            });

            // Check template
            _templates.Add(new ElementTemplate
            {
                Name = "check",
                DisplayName = "Check",
                Description = "Validation check for extracted data",
                Template = new XElement("check",
                    new XAttribute("sourceRef", "extracted_value"),
                    new XAttribute("expect", "expected_value")),
                AllowedParents = new List<string> { "checks" }
            });

            // Session template
            _templates.Add(new ElementTemplate
            {
                Name = "session",
                DisplayName = "Session",
                Description = "Connection session configuration",
                Template = new XElement("session",
                    new XAttribute("name", "NEW_SESSION"),
                    new XAttribute("type", "ssh"),
                    new XAttribute("host", "localhost"),
                    new XAttribute("port", "22"),
                    new XAttribute("user", "user"),
                    new XAttribute("password", "password")),
                AllowedParents = new List<string> { "sessions" }
            });
        }

        private void GenerateUniqueIds(XElement element)
        {
            var idAttr = element.Attribute("id");
            if (idAttr != null && idAttr.Value.Contains("placeholder"))
            {
                var timestamp = DateTimeOffset.Now.Ticks.ToString("x")[^8..]; // Last 8 chars of hex timestamp
                idAttr.Value = idAttr.Value.Replace("placeholder", timestamp);
            }

            var nameAttr = element.Attribute("name");
            if (nameAttr != null && nameAttr.Value.Contains("placeholder"))
            {
                var timestamp = DateTimeOffset.Now.Ticks.ToString("x")[^8..];
                nameAttr.Value = nameAttr.Value.Replace("placeholder", timestamp);
            }

            // Recursively process children
            foreach (var child in element.Elements())
            {
                GenerateUniqueIds(child);
            }
        }
    }
}