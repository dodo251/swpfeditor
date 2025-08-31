using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SwpfEditor.App.Services;

public static class XmlFileService
{
    /// <summary>
    /// Load XML document with UTF-8 encoding and line information
    /// </summary>
    public static XDocument LoadXmlFile(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, true);
        return XDocument.Load(reader, LoadOptions.SetLineInfo);
    }

    /// <summary>
    /// Save XML document with proper formatting for minimal diffs
    /// </summary>
    public static void SaveXmlFile(XDocument document, string path)
    {
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false), // UTF-8 without BOM
            Indent = true,
            IndentChars = "  ", // 2 spaces as per SRS requirement
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = false
        };

        using var writer = XmlWriter.Create(path, settings);
        document.Save(writer);
    }

    /// <summary>
    /// Create element header for display (alias > id > element name)
    /// </summary>
    public static string CreateElementHeader(XElement element)
    {
        var id = element.Attribute("id")?.Value;
        var alias = element.Attribute("alias")?.Value;
        
        if (!string.IsNullOrWhiteSpace(alias)) 
            return $"{element.Name.LocalName} ({alias})";
        
        if (!string.IsNullOrWhiteSpace(id)) 
            return $"{element.Name.LocalName} ({id})";
            
        return element.Name.LocalName;
    }
}