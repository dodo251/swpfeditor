using System.Xml.Linq;
using SwpfEditor.Domain.Models;

namespace SwpfEditor.Infrastructure.Mapping;

public static class XmlTestConfigurationMapper
{
    public static TestConfiguration FromXElement(XElement element)
    {
        var config = new TestConfiguration();

        var uutElement = element.Element("UUT");
        if (uutElement != null)
        {
            config.UUT = MapUUT(uutElement);
        }

        config.LogDirectory = element.Element("LogDirectory")?.Value;

        var sshListElement = element.Element("SshList");
        if (sshListElement != null)
        {
            config.SshList = MapSshList(sshListElement);
        }

        var telnetListElement = element.Element("TelnetList");
        if (telnetListElement != null)
        {
            config.TelnetList = MapTelnetList(telnetListElement);
        }

        var httpListElement = element.Element("HttpList");
        if (httpListElement != null)
        {
            config.HttpList = MapHttpList(httpListElement);
        }

        var variablesElement = element.Element("Variables");
        if (variablesElement != null)
        {
            config.Variables = MapVariables(variablesElement);
        }

        var constantsElement = element.Element("Constants");
        if (constantsElement != null)
        {
            config.Constants = MapConstants(constantsElement);
        }

        var inputsElement = element.Element("Inputs");
        if (inputsElement != null)
        {
            config.Inputs = MapInputs(inputsElement);
        }

        return config;
    }

    public static XElement ToXElement(TestConfiguration config)
    {
        var element = new XElement("TestConfiguration");

        if (config.UUT != null)
            element.Add(ToXElement(config.UUT));

        if (!string.IsNullOrEmpty(config.LogDirectory))
            element.Add(new XElement("LogDirectory", config.LogDirectory));

        if (config.SshList != null)
            element.Add(ToXElement(config.SshList));

        if (config.TelnetList != null)
            element.Add(ToXElement(config.TelnetList));

        if (config.HttpList != null)
            element.Add(ToXElement(config.HttpList));

        if (config.Variables != null)
            element.Add(ToXElement(config.Variables));

        if (config.Constants != null)
            element.Add(ToXElement(config.Constants));

        if (config.Inputs != null)
            element.Add(ToXElement(config.Inputs));

        return element;
    }

    private static UUT MapUUT(XElement element)
    {
        var uut = new UUT
        {
            Name = element.Element("Name")?.Value ?? string.Empty,
            Type = element.Element("Type")?.Value ?? string.Empty,
            Host = element.Element("Host")?.Value,
            User = element.Element("User")?.Value,
            Password = element.Element("Password")?.Value,
            Prompt = element.Element("Prompt")?.Value
        };

        var portElement = element.Element("Port");
        if (portElement != null && int.TryParse(portElement.Value, out var port))
        {
            uut.Port = port;
        }

        return uut;
    }

    private static XElement ToXElement(UUT uut)
    {
        var element = new XElement("UUT");
        element.Add(new XElement("Name", uut.Name));
        element.Add(new XElement("Type", uut.Type));
        
        if (!string.IsNullOrEmpty(uut.Host))
            element.Add(new XElement("Host", uut.Host));
        
        if (uut.Port.HasValue)
            element.Add(new XElement("Port", uut.Port.Value));
        
        if (!string.IsNullOrEmpty(uut.User))
            element.Add(new XElement("User", uut.User));
        
        if (!string.IsNullOrEmpty(uut.Password))
            element.Add(new XElement("Password", uut.Password));
        
        if (!string.IsNullOrEmpty(uut.Prompt))
            element.Add(new XElement("Prompt", uut.Prompt));

        return element;
    }

    private static SshList MapSshList(XElement element)
    {
        var sshList = new SshList();
        
        foreach (var sshElement in element.Elements("Ssh"))
        {
            sshList.Connections.Add(new SshConnection
            {
                Name = sshElement.Attribute("Name")?.Value ?? string.Empty,
                Host = sshElement.Attribute("Host")?.Value ?? string.Empty,
                Port = int.TryParse(sshElement.Attribute("Port")?.Value, out var port) ? port : 22,
                User = sshElement.Attribute("User")?.Value ?? string.Empty,
                Password = sshElement.Attribute("Password")?.Value ?? string.Empty,
                Prompt = sshElement.Attribute("Prompt")?.Value ?? "$"
            });
        }

        return sshList;
    }

    private static XElement ToXElement(SshList sshList)
    {
        var element = new XElement("SshList");
        
        foreach (var ssh in sshList.Connections)
        {
            var sshElement = new XElement("Ssh");
            sshElement.SetAttributeValue("Name", ssh.Name);
            sshElement.SetAttributeValue("Host", ssh.Host);
            sshElement.SetAttributeValue("Port", ssh.Port);
            sshElement.SetAttributeValue("User", ssh.User);
            sshElement.SetAttributeValue("Password", ssh.Password);
            sshElement.SetAttributeValue("Prompt", ssh.Prompt);
            element.Add(sshElement);
        }

        return element;
    }

    private static TelnetList MapTelnetList(XElement element)
    {
        var telnetList = new TelnetList();
        
        foreach (var telnetElement in element.Elements("Telnet"))
        {
            telnetList.Connections.Add(new TelnetConnection
            {
                Name = telnetElement.Attribute("Name")?.Value ?? string.Empty,
                Host = telnetElement.Attribute("Host")?.Value ?? string.Empty,
                Port = int.TryParse(telnetElement.Attribute("Port")?.Value, out var port) ? port : 23,
                User = telnetElement.Attribute("User")?.Value ?? string.Empty,
                Password = telnetElement.Attribute("Password")?.Value ?? string.Empty,
                Prompt = telnetElement.Attribute("Prompt")?.Value ?? ">"
            });
        }

        return telnetList;
    }

    private static XElement ToXElement(TelnetList telnetList)
    {
        var element = new XElement("TelnetList");
        
        foreach (var telnet in telnetList.Connections)
        {
            var telnetElement = new XElement("Telnet");
            telnetElement.SetAttributeValue("Name", telnet.Name);
            telnetElement.SetAttributeValue("Host", telnet.Host);
            telnetElement.SetAttributeValue("Port", telnet.Port);
            telnetElement.SetAttributeValue("User", telnet.User);
            telnetElement.SetAttributeValue("Password", telnet.Password);
            telnetElement.SetAttributeValue("Prompt", telnet.Prompt);
            element.Add(telnetElement);
        }

        return element;
    }

    private static HttpList MapHttpList(XElement element)
    {
        var httpList = new HttpList();
        
        foreach (var httpElement in element.Elements("Http"))
        {
            httpList.Connections.Add(new HttpConnection
            {
                Name = httpElement.Attribute("Name")?.Value ?? string.Empty,
                BaseUrl = httpElement.Attribute("BaseUrl")?.Value ?? string.Empty
            });
        }

        return httpList;
    }

    private static XElement ToXElement(HttpList httpList)
    {
        var element = new XElement("HttpList");
        
        foreach (var http in httpList.Connections)
        {
            var httpElement = new XElement("Http");
            httpElement.SetAttributeValue("Name", http.Name);
            httpElement.SetAttributeValue("BaseUrl", http.BaseUrl);
            element.Add(httpElement);
        }

        return element;
    }

    private static Variables MapVariables(XElement element)
    {
        var variables = new Variables();
        
        foreach (var variableElement in element.Elements("Variable"))
        {
            variables.VariableList.Add(new Variable
            {
                Name = variableElement.Attribute("Name")?.Value ?? string.Empty,
                Value = variableElement.Attribute("Value")?.Value ?? string.Empty
            });
        }

        return variables;
    }

    private static XElement ToXElement(Variables variables)
    {
        var element = new XElement("Variables");
        
        foreach (var variable in variables.VariableList)
        {
            var variableElement = new XElement("Variable");
            variableElement.SetAttributeValue("Name", variable.Name);
            variableElement.SetAttributeValue("Value", variable.Value);
            element.Add(variableElement);
        }

        return element;
    }

    private static Constants MapConstants(XElement element)
    {
        var constants = new Constants();
        
        foreach (var constantElement in element.Elements("Constant"))
        {
            constants.ConstantList.Add(new Constant
            {
                Name = constantElement.Attribute("Name")?.Value ?? string.Empty,
                Value = constantElement.Attribute("Value")?.Value ?? string.Empty
            });
        }

        return constants;
    }

    private static XElement ToXElement(Constants constants)
    {
        var element = new XElement("Constants");
        
        foreach (var constant in constants.ConstantList)
        {
            var constantElement = new XElement("Constant");
            constantElement.SetAttributeValue("Name", constant.Name);
            constantElement.SetAttributeValue("Value", constant.Value);
            element.Add(constantElement);
        }

        return element;
    }

    private static Inputs MapInputs(XElement element)
    {
        var inputs = new Inputs();
        
        foreach (var inputElement in element.Elements("Input"))
        {
            inputs.InputList.Add(new Input
            {
                Name = inputElement.Attribute("Name")?.Value ?? string.Empty,
                Value = inputElement.Attribute("Value")?.Value ?? string.Empty
            });
        }

        return inputs;
    }

    private static XElement ToXElement(Inputs inputs)
    {
        var element = new XElement("Inputs");
        
        foreach (var input in inputs.InputList)
        {
            var inputElement = new XElement("Input");
            inputElement.SetAttributeValue("Name", input.Name);
            inputElement.SetAttributeValue("Value", input.Value);
            element.Add(inputElement);
        }

        return element;
    }
}