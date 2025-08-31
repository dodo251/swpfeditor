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

namespace SwpfEditor.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private XDocument? _currentDoc;
        private string? _currentPath;

        public MainWindow()
        {
            InitializeComponent();
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
            
            // Save with proper formatting for minimal diffs
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false), // UTF-8 without BOM
                Indent = true,
                IndentChars = "  ", // 2 spaces as per SRS
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = false
            };
            
            using var writer = XmlWriter.Create(_currentPath!, settings);
            _currentDoc.Save(writer);
        }

        private void BtnValidate_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("验证功能占位", "Validate");
        }

        private void LoadFile(string path)
        {
            try
            {
                using var sr = new StreamReader(path, Encoding.UTF8, true);
                var xml = XDocument.Load(sr, LoadOptions.SetLineInfo);
                _currentDoc = xml;
                _currentPath = path;
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
            var id = el.Attribute("id")?.Value;
            var alias = el.Attribute("alias")?.Value;
            if (!string.IsNullOrWhiteSpace(alias)) return $"{el.Name.LocalName} ({alias})";
            if (!string.IsNullOrWhiteSpace(id)) return $"{el.Name.LocalName} ({id})";
            return el.Name.LocalName;
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