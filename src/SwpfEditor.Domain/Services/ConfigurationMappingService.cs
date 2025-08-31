using SwpfEditor.Domain.Models;
using System.Xml.Linq;

namespace SwpfEditor.Domain.Services;

/// <summary>
/// Maps between TestConfiguration XML and POCO models
/// </summary>
public class ConfigurationMappingService
{
    /// <summary>
    /// Convert TestConfiguration POCO to XDocument
    /// </summary>
    public XDocument ConfigurationToXml(TestConfiguration config)
    {
        var root = new XElement(XName.Get("TestConfiguration", "http://sms.test/config/1.0"));

        // Add UUT
        if (config.UUT != null && config.UUT.Properties.Any())
        {
            var uutElement = new XElement("UUT");
            foreach (var prop in config.UUT.Properties)
            {
                uutElement.Add(new XElement(prop.Key, prop.Value));
            }
            root.Add(uutElement);
        }

        // Add LogDirectory
        if (!string.IsNullOrEmpty(config.LogDirectory))
        {
            root.Add(new XElement("LogDirectory", config.LogDirectory));
        }

        // Add SSH connections
        if (config.SshList.Any())
        {
            var sshListElement = new XElement("SshList");
            foreach (var conn in config.SshList)
            {
                var connElement = new XElement("SshConnection",
                    new XAttribute("Name", conn.Name),
                    new XAttribute("Host", conn.Host),
                    new XAttribute("Port", conn.Port));

                if (!string.IsNullOrEmpty(conn.Username))
                    connElement.Add(new XAttribute("Username", conn.Username));
                if (!string.IsNullOrEmpty(conn.Password))
                    connElement.Add(new XAttribute("Password", conn.Password));
                if (!string.IsNullOrEmpty(conn.PrivateKeyPath))
                    connElement.Add(new XAttribute("PrivateKeyPath", conn.PrivateKeyPath));
                if (!string.IsNullOrEmpty(conn.Prompt))
                    connElement.Add(new XAttribute("Prompt", conn.Prompt));

                sshListElement.Add(connElement);
            }
            root.Add(sshListElement);
        }

        // Add Telnet connections
        if (config.TelnetList.Any())
        {
            var telnetListElement = new XElement("TelnetList");
            foreach (var conn in config.TelnetList)
            {
                var connElement = new XElement("TelnetConnection",
                    new XAttribute("Name", conn.Name),
                    new XAttribute("Host", conn.Host),
                    new XAttribute("Port", conn.Port));

                if (!string.IsNullOrEmpty(conn.Username))
                    connElement.Add(new XAttribute("Username", conn.Username));
                if (!string.IsNullOrEmpty(conn.Password))
                    connElement.Add(new XAttribute("Password", conn.Password));
                if (!string.IsNullOrEmpty(conn.Prompt))
                    connElement.Add(new XAttribute("Prompt", conn.Prompt));

                telnetListElement.Add(connElement);
            }
            root.Add(telnetListElement);
        }

        // Add HTTP connections
        if (config.HttpList.Any())
        {
            var httpListElement = new XElement("HttpList");
            foreach (var conn in config.HttpList)
            {
                var connElement = new XElement("HttpConnection",
                    new XAttribute("Name", conn.Name),
                    new XAttribute("BaseUrl", conn.BaseUrl));

                if (!string.IsNullOrEmpty(conn.Username))
                    connElement.Add(new XAttribute("Username", conn.Username));
                if (!string.IsNullOrEmpty(conn.Password))
                    connElement.Add(new XAttribute("Password", conn.Password));
                if (conn.Timeout.HasValue)
                    connElement.Add(new XAttribute("Timeout", conn.Timeout.Value));

                httpListElement.Add(connElement);
            }
            root.Add(httpListElement);
        }

        // Add Variables
        if (config.Variables.Any())
        {
            var variablesElement = new XElement("Variables");
            foreach (var variable in config.Variables)
            {
                variablesElement.Add(new XElement("Variable",
                    new XAttribute("Name", variable.Name),
                    new XAttribute("Value", variable.Value)));
            }
            root.Add(variablesElement);
        }

        // Add Constants
        if (config.Constants.Any())
        {
            var constantsElement = new XElement("Constants");
            foreach (var constant in config.Constants)
            {
                constantsElement.Add(new XElement("Constant",
                    new XAttribute("Name", constant.Name),
                    new XAttribute("Value", constant.Value)));
            }
            root.Add(constantsElement);
        }

        // Add Inputs
        if (config.Inputs.Any())
        {
            var inputsElement = new XElement("Inputs");
            foreach (var input in config.Inputs)
            {
                var inputElement = new XElement("Input",
                    new XAttribute("Name", input.Name),
                    new XAttribute("Value", input.Value));

                if (input.Type != "string")
                    inputElement.Add(new XAttribute("Type", input.Type));

                inputsElement.Add(inputElement);
            }
            root.Add(inputsElement);
        }

        return new XDocument(root);
    }

    /// <summary>
    /// Convert XDocument to TestConfiguration POCO
    /// </summary>
    public TestConfiguration XmlToConfiguration(XDocument document)
    {
        var root = document.Root ?? throw new ArgumentException("Document has no root element");
        var config = new TestConfiguration();

        // Parse UUT
        var uutElement = root.Element(XName.Get("UUT", root.Name.NamespaceName));
        if (uutElement != null)
        {
            config.UUT = new UUT();
            foreach (var child in uutElement.Elements())
            {
                config.UUT.Properties[child.Name.LocalName] = child.Value;
            }
        }

        // Parse LogDirectory
        var logDirElement = root.Element(XName.Get("LogDirectory", root.Name.NamespaceName));
        if (logDirElement != null)
        {
            config.LogDirectory = logDirElement.Value;
        }

        // Parse SSH connections
        var sshListElement = root.Element(XName.Get("SshList", root.Name.NamespaceName));
        if (sshListElement != null)
        {
            foreach (var connElement in sshListElement.Elements(XName.Get("SshConnection", root.Name.NamespaceName)))
            {
                config.SshList.Add(new SshConnection
                {
                    Name = connElement.Attribute("Name")?.Value ?? "",
                    Host = connElement.Attribute("Host")?.Value ?? "",
                    Port = int.TryParse(connElement.Attribute("Port")?.Value, out var port) ? port : 22,
                    Username = connElement.Attribute("Username")?.Value,
                    Password = connElement.Attribute("Password")?.Value,
                    PrivateKeyPath = connElement.Attribute("PrivateKeyPath")?.Value,
                    Prompt = connElement.Attribute("Prompt")?.Value
                });
            }
        }

        // Parse Telnet connections
        var telnetListElement = root.Element(XName.Get("TelnetList", root.Name.NamespaceName));
        if (telnetListElement != null)
        {
            foreach (var connElement in telnetListElement.Elements(XName.Get("TelnetConnection", root.Name.NamespaceName)))
            {
                config.TelnetList.Add(new TelnetConnection
                {
                    Name = connElement.Attribute("Name")?.Value ?? "",
                    Host = connElement.Attribute("Host")?.Value ?? "",
                    Port = int.TryParse(connElement.Attribute("Port")?.Value, out var port) ? port : 23,
                    Username = connElement.Attribute("Username")?.Value,
                    Password = connElement.Attribute("Password")?.Value,
                    Prompt = connElement.Attribute("Prompt")?.Value
                });
            }
        }

        // Parse HTTP connections
        var httpListElement = root.Element(XName.Get("HttpList", root.Name.NamespaceName));
        if (httpListElement != null)
        {
            foreach (var connElement in httpListElement.Elements(XName.Get("HttpConnection", root.Name.NamespaceName)))
            {
                config.HttpList.Add(new HttpConnection
                {
                    Name = connElement.Attribute("Name")?.Value ?? "",
                    BaseUrl = connElement.Attribute("BaseUrl")?.Value ?? "",
                    Username = connElement.Attribute("Username")?.Value,
                    Password = connElement.Attribute("Password")?.Value,
                    Timeout = int.TryParse(connElement.Attribute("Timeout")?.Value, out var timeout) ? timeout : null
                });
            }
        }

        // Parse Variables
        var variablesElement = root.Element(XName.Get("Variables", root.Name.NamespaceName));
        if (variablesElement != null)
        {
            foreach (var varElement in variablesElement.Elements(XName.Get("Variable", root.Name.NamespaceName)))
            {
                config.Variables.Add(new Variable
                {
                    Name = varElement.Attribute("Name")?.Value ?? "",
                    Value = varElement.Attribute("Value")?.Value ?? ""
                });
            }
        }

        // Parse Constants
        var constantsElement = root.Element(XName.Get("Constants", root.Name.NamespaceName));
        if (constantsElement != null)
        {
            foreach (var constElement in constantsElement.Elements(XName.Get("Constant", root.Name.NamespaceName)))
            {
                config.Constants.Add(new Constant
                {
                    Name = constElement.Attribute("Name")?.Value ?? "",
                    Value = constElement.Attribute("Value")?.Value ?? ""
                });
            }
        }

        // Parse Inputs
        var inputsElement = root.Element(XName.Get("Inputs", root.Name.NamespaceName));
        if (inputsElement != null)
        {
            foreach (var inputElement in inputsElement.Elements(XName.Get("Input", root.Name.NamespaceName)))
            {
                config.Inputs.Add(new Input
                {
                    Name = inputElement.Attribute("Name")?.Value ?? "",
                    Value = inputElement.Attribute("Value")?.Value ?? "",
                    Type = inputElement.Attribute("Type")?.Value ?? "string"
                });
            }
        }

        return config;
    }
}