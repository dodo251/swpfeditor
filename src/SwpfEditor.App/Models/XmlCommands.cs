using System;
using System.Xml.Linq;
using SwpfEditor.App.Services;

namespace SwpfEditor.App.Models
{
    /// <summary>
    /// Command for changing XML attribute values
    /// </summary>
    public class ChangeAttributeCommand : IUndoableCommand
    {
        private readonly XElement _element;
        private readonly XName _attributeName;
        private readonly string? _newValue;
        private readonly string? _oldValue;

        public string Description { get; }

        public ChangeAttributeCommand(XElement element, XName attributeName, string? newValue)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _attributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
            _newValue = newValue;
            _oldValue = element.Attribute(attributeName)?.Value;
            
            Description = $"Change {attributeName.LocalName} from '{_oldValue}' to '{_newValue}'";
        }

        public void Execute()
        {
            if (_newValue == null)
            {
                _element.Attribute(_attributeName)?.Remove();
            }
            else
            {
                _element.SetAttributeValue(_attributeName, _newValue);
            }
        }

        public void Undo()
        {
            if (_oldValue == null)
            {
                _element.Attribute(_attributeName)?.Remove();
            }
            else
            {
                _element.SetAttributeValue(_attributeName, _oldValue);
            }
        }
    }

    /// <summary>
    /// Command for adding XML elements
    /// </summary>
    public class AddElementCommand : IUndoableCommand
    {
        private readonly XElement _parent;
        private readonly XElement _element;
        private readonly XElement? _insertAfter;

        public string Description { get; }

        public AddElementCommand(XElement parent, XElement element, XElement? insertAfter = null)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _insertAfter = insertAfter;
            
            Description = $"Add <{element.Name.LocalName}> to <{parent.Name.LocalName}>";
        }

        public void Execute()
        {
            if (_insertAfter == null)
            {
                _parent.Add(_element);
            }
            else
            {
                _insertAfter.AddAfterSelf(_element);
            }
        }

        public void Undo()
        {
            _element.Remove();
        }
    }

    /// <summary>
    /// Command for removing XML elements
    /// </summary>
    public class RemoveElementCommand : IUndoableCommand
    {
        private readonly XElement _element;
        private readonly XElement? _parent;
        private readonly XElement? _previousSibling;
        private readonly XElement? _nextSibling;

        public string Description { get; }

        public RemoveElementCommand(XElement element)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _parent = element.Parent;
            _previousSibling = element.ElementsBeforeSelf().LastOrDefault();
            _nextSibling = element.ElementsAfterSelf().FirstOrDefault();
            
            Description = $"Remove <{element.Name.LocalName}>";
        }

        public void Execute()
        {
            _element.Remove();
        }

        public void Undo()
        {
            if (_parent == null) return;

            if (_nextSibling != null)
            {
                _nextSibling.AddBeforeSelf(_element);
            }
            else if (_previousSibling != null)
            {
                _previousSibling.AddAfterSelf(_element);
            }
            else
            {
                _parent.Add(_element);
            }
        }
    }

    /// <summary>
    /// Command for moving XML elements
    /// </summary>
    public class MoveElementCommand : IUndoableCommand
    {
        private readonly XElement _element;
        private readonly XElement _newParent;
        private readonly XElement? _insertAfter;
        private readonly XElement? _oldParent;
        private readonly XElement? _oldPreviousSibling;
        private readonly XElement? _oldNextSibling;

        public string Description { get; }

        public MoveElementCommand(XElement element, XElement newParent, XElement? insertAfter = null)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _newParent = newParent ?? throw new ArgumentNullException(nameof(newParent));
            _insertAfter = insertAfter;
            
            // Store old position
            _oldParent = element.Parent;
            _oldPreviousSibling = element.ElementsBeforeSelf().LastOrDefault();
            _oldNextSibling = element.ElementsAfterSelf().FirstOrDefault();
            
            Description = $"Move <{element.Name.LocalName}> to <{newParent.Name.LocalName}>";
        }

        public void Execute()
        {
            _element.Remove();
            
            if (_insertAfter == null)
            {
                _newParent.Add(_element);
            }
            else
            {
                _insertAfter.AddAfterSelf(_element);
            }
        }

        public void Undo()
        {
            if (_oldParent == null) return;

            _element.Remove();

            if (_oldNextSibling != null)
            {
                _oldNextSibling.AddBeforeSelf(_element);
            }
            else if (_oldPreviousSibling != null)
            {
                _oldPreviousSibling.AddAfterSelf(_element);
            }
            else
            {
                _oldParent.Add(_element);
            }
        }
    }
}