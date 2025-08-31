using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using GongSolutions.Wpf.DragDrop;
using SwpfEditor.App.Models;
using SwpfEditor.App.Services;
using SwpfEditor.App.Validation;

namespace SwpfEditor.App.Services
{
    /// <summary>
    /// Drag and drop handler for XML elements and templates
    /// </summary>
    public class XmlDragDropHandler : IDropTarget, IDragSource
    {
        private readonly IValidationService _validationService;
        private readonly ITemplateService _templateService;
        private readonly IUndoRedoService _undoRedoService;
        private readonly ILoggingService _loggingService;
        private readonly Func<XElement, TreeViewItem?> _findTreeViewItem;
        private readonly Action _refreshTree;

        public XmlDragDropHandler(
            IValidationService validationService,
            ITemplateService templateService,
            IUndoRedoService undoRedoService,
            ILoggingService loggingService,
            Func<XElement, TreeViewItem?> findTreeViewItem,
            Action refreshTree)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _findTreeViewItem = findTreeViewItem ?? throw new ArgumentNullException(nameof(findTreeViewItem));
            _refreshTree = refreshTree ?? throw new ArgumentNullException(nameof(refreshTree));
        }

        #region IDragSource Implementation

        public void StartDrag(IDragInfo dragInfo)
        {
            if (dragInfo.SourceItem is TreeViewItem tvi && tvi.Tag is XElement element)
            {
                dragInfo.Data = element;
                dragInfo.Effects = DragDropEffects.Move;
            }
            else if (dragInfo.SourceItem is ElementTemplate template)
            {
                dragInfo.Data = template;
                dragInfo.Effects = DragDropEffects.Copy;
            }
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return (dragInfo.SourceItem is TreeViewItem tvi && tvi.Tag is XElement) ||
                   dragInfo.SourceItem is ElementTemplate;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            // Called when drag operation completes successfully
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            // Called when drag operation finishes
        }

        public void DragCancelled()
        {
            // Called when drag operation is cancelled
        }

        #endregion

        #region IDropTarget Implementation

        public void DragEnter(IDropInfo dropInfo)
        {
            UpdateDropInfo(dropInfo);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            UpdateDropInfo(dropInfo);
        }

        public void DragLeave(IDropInfo dropInfo)
        {
            // Clear any visual feedback
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (!dropInfo.Effects.HasFlag(DragDropEffects.Move) && 
                !dropInfo.Effects.HasFlag(DragDropEffects.Copy))
            {
                return;
            }

            try
            {
                if (dropInfo.Data is XElement sourceElement)
                {
                    HandleElementDrop(dropInfo, sourceElement);
                }
                else if (dropInfo.Data is ElementTemplate template)
                {
                    HandleTemplateDrop(dropInfo, template);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Drag and drop operation failed", ex.Message);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void UpdateDropInfo(IDropInfo dropInfo)
        {
            // Default to no drop
            dropInfo.Effects = DragDropEffects.None;
            dropInfo.DropTargetAdorner = null;

            if (!(dropInfo.VisualTarget is TreeView))
                return;

            var targetItem = GetTargetTreeViewItem(dropInfo);
            if (targetItem?.Tag is not XElement targetElement)
                return;

            // Determine what we're dragging
            string? childElementName = null;
            
            if (dropInfo.Data is XElement sourceElement)
            {
                // Don't allow dropping on self or descendants
                if (IsDescendantOrSelf(sourceElement, targetElement))
                    return;
                    
                childElementName = sourceElement.Name.LocalName;
                dropInfo.Effects = DragDropEffects.Move;
            }
            else if (dropInfo.Data is ElementTemplate template)
            {
                childElementName = template.Template.Name.LocalName;
                dropInfo.Effects = DragDropEffects.Copy;
            }

            if (childElementName == null)
                return;

            // Check if this is a valid parent-child relationship
            if (!_validationService.IsValidChild(targetElement.Name.LocalName, childElementName))
            {
                // Show invalid drop visual feedback
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                return;
            }

            // Valid drop - set visual feedback
            dropInfo.Effects = dropInfo.Data is XElement ? DragDropEffects.Move : DragDropEffects.Copy;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
        }

        private void HandleElementDrop(IDropInfo dropInfo, XElement sourceElement)
        {
            var targetItem = GetTargetTreeViewItem(dropInfo);
            if (targetItem?.Tag is not XElement targetElement)
                return;

            // Find insert position
            XElement? insertAfter = null;
            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
            {
                insertAfter = targetElement;
                targetElement = targetElement.Parent ?? targetElement;
            }

            // Create move command
            var command = new MoveElementCommand(sourceElement, targetElement, insertAfter);
            _undoRedoService.ExecuteCommand(command);
            
            _refreshTree();
            _loggingService.LogInfo($"Moved element <{sourceElement.Name.LocalName}> to <{targetElement.Name.LocalName}>");
        }

        private void HandleTemplateDrop(IDropInfo dropInfo, ElementTemplate template)
        {
            var targetItem = GetTargetTreeViewItem(dropInfo);
            if (targetItem?.Tag is not XElement targetElement)
                return;

            // Create element from template
            var newElement = _templateService.CreateElementFromTemplate(template.Name);

            // Find insert position
            XElement? insertAfter = null;
            if (dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
            {
                insertAfter = targetElement;
                targetElement = targetElement.Parent ?? targetElement;
            }

            // Create add command
            var command = new AddElementCommand(targetElement, newElement, insertAfter);
            _undoRedoService.ExecuteCommand(command);
            
            _refreshTree();
            _loggingService.LogInfo($"Added element <{newElement.Name.LocalName}> from template to <{targetElement.Name.LocalName}>");
        }

        private TreeViewItem? GetTargetTreeViewItem(IDropInfo dropInfo)
        {
            return dropInfo.VisualTargetItem as TreeViewItem;
        }

        private bool IsDescendantOrSelf(XElement ancestor, XElement element)
        {
            var current = element;
            while (current != null)
            {
                if (ReferenceEquals(current, ancestor))
                    return true;
                current = current.Parent;
            }
            return false;
        }
    }
}