using System;
using System.Collections.Generic;
using System.Linq;

namespace SwpfEditor.App.Validation
{
    /// <summary>
    /// Service for validating XML element parent-child relationships based on SRS specifications
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Check if a child element type can be added to a parent element type
        /// </summary>
        bool IsValidChild(string parentElementName, string childElementName);

        /// <summary>
        /// Get allowed child element types for a parent element
        /// </summary>
        IEnumerable<string> GetAllowedChildren(string parentElementName);

        /// <summary>
        /// Get a validation error message if the relationship is invalid
        /// </summary>
        string? GetValidationMessage(string parentElementName, string childElementName);
    }

    /// <summary>
    /// Implementation of validation service based on XSD content model from SRS
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly Dictionary<string, HashSet<string>> _allowedChildren;

        public ValidationService()
        {
            // Based on SRS Section 8.1 and Appendix-XSD Section 2.5
            _allowedChildren = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                // test: meta|description|displayOrder|sessions|config|functions|steps|testGroups|sections
                ["test"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "meta", "description", "displayOrder", "sessions", "config", 
                    "functions", "steps", "testGroups", "sections"
                },
                
                // steps ⇒ step*
                ["steps"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "step" },
                
                // step ⇒ params? headers? extracts? interaction?
                ["step"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                { 
                    "params", "headers", "extracts", "interaction" 
                },
                
                // extracts ⇒ extract*
                ["extracts"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "extract" },
                
                // extract ⇒ checks?
                ["extract"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "checks" },
                
                // checks ⇒ check*
                ["checks"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "check" },
                
                // sections ⇒ section*
                ["sections"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "section" },
                
                // section ⇒ refs?
                ["section"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "refs" },
                
                // refs ⇒ ref*
                ["refs"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ref" },
                
                // sessions ⇒ session*
                ["sessions"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "session" },
                
                // config can contain various configuration elements
                ["config"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                { 
                    "setting", "variable", "constant" 
                },
                
                // functions ⇒ function*
                ["functions"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "function" },
                
                // testGroups ⇒ testGroup*
                ["testGroups"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "testGroup" },
                
                // params ⇒ param*
                ["params"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "param" },
                
                // headers ⇒ header*
                ["headers"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "header" }
            };
        }

        public bool IsValidChild(string parentElementName, string childElementName)
        {
            if (string.IsNullOrEmpty(parentElementName) || string.IsNullOrEmpty(childElementName))
                return false;

            return _allowedChildren.TryGetValue(parentElementName, out var allowedChildren) &&
                   allowedChildren.Contains(childElementName);
        }

        public IEnumerable<string> GetAllowedChildren(string parentElementName)
        {
            if (string.IsNullOrEmpty(parentElementName))
                return Enumerable.Empty<string>();

            return _allowedChildren.TryGetValue(parentElementName, out var allowedChildren) 
                ? allowedChildren 
                : Enumerable.Empty<string>();
        }

        public string? GetValidationMessage(string parentElementName, string childElementName)
        {
            if (IsValidChild(parentElementName, childElementName))
                return null;

            var allowedChildren = GetAllowedChildren(parentElementName).ToList();
            if (!allowedChildren.Any())
            {
                return $"Element '{parentElementName}' cannot contain any child elements.";
            }

            return $"Element '{parentElementName}' cannot contain '{childElementName}'. " +
                   $"Allowed children: {string.Join(", ", allowedChildren)}";
        }
    }
}