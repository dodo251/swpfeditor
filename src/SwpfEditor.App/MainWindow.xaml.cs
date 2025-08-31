using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using SwpfEditor.App.Services;
using SwpfEditor.App.ViewModels;

namespace SwpfEditor.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XDocument? _currentDoc;
        private string? _currentPath;
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // Auto load sample if exists
            var sample = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "test.xml");
            if (File.Exists(sample))
            {
                LoadFile(sample);
            }
        }

        private void BtnNew_OnClick(object sender, RoutedEventArgs e)
        {
            _currentDoc = new XDocument(new XElement("test"));
            _currentPath = null;
            RefreshTree();
        }

        private void BtnOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "XML (*.xml)|*.xml" };
            if (dlg.ShowDialog() == true)
            {
                LoadFile(dlg.FileName);
            }
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentDoc == null) return;
            if (string.IsNullOrEmpty(_currentPath))
            {
                var dlg = new SaveFileDialog { Filter = "XML (*.xml)|*.xml", FileName = "test.xml" };
                if (dlg.ShowDialog() != true) return;
                _currentPath = dlg.FileName;
            }
            
            try
            {
                _viewModel.SaveDocument(_currentPath);
                MessageBox.Show("保存成功", "保存", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnValidate_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("验证功能占位", "Validate");
        }

        private void LoadFile(string path)
        {
            try
            {
                var xml = XmlFileService.LoadXmlFile(path);
                _currentDoc = xml;
                _currentPath = path;
                _viewModel.LoadDocument(xml, path);
                RefreshTree();
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
            if (_currentDoc?.Root == null) return;
            ScriptTree.Items.Add(CreateTreeItem(_currentDoc.Root));
        }

        private TreeViewItem CreateTreeItem(XElement element)
        {
            var tvi = new TreeViewItem { Header = ElementHeader(element), Tag = element, IsExpanded = true };
            foreach (var child in element.Elements())
            {
                tvi.Items.Add(CreateTreeItem(child));
            }
            return tvi;
        }

        private string ElementHeader(XElement el)
        {
            return XmlFileService.CreateElementHeader(el);
        }

        private void ScriptTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PropertiesPanel.Children.Clear();
            if (ScriptTree.SelectedItem is not TreeViewItem tvi || tvi.Tag is not XElement el) return;
            AddLabel($"元素: <{el.Name.LocalName}>");
            foreach (var attr in el.Attributes())
            {
                AddAttrEditor(el, attr);
            }
        }

        private void AddLabel(string text)
        {
            PropertiesPanel.Children.Add(new TextBlock { Text = text, Margin = new Thickness(0, 8, 0, 4), FontWeight = FontWeights.Bold });
        }

        private void AddAttrEditor(XElement owner, XAttribute attr)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            panel.Children.Add(new TextBlock { Text = attr.Name.LocalName, Width = 120, VerticalAlignment = VerticalAlignment.Center });
            var tb = new TextBox { Text = attr.Value, Width = 200, Tag = (owner, attr.Name) };
            tb.TextChanged += AttrTextChanged;
            panel.Children.Add(tb);
            PropertiesPanel.Children.Add(panel);
        }

        private void AttrTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Tag is ValueTuple<XElement, XName> tag)
            {
                var (owner, name) = tag;
                var xa = owner.Attribute(name);
                if (xa != null && xa.Value != tb.Text)
                {
                    xa.Value = tb.Text; // In-place keeps ordering
                    // Update header of tree node
                    if (ScriptTree.SelectedItem is TreeViewItem sel && sel.Tag == owner)
                    {
                        sel.Header = ElementHeader(owner);
                    }
                }
            }
        }
    }
}