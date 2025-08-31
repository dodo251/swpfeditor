namespace SwpfEditor.Domain.Models;

public class TestConfiguration
{
    public UUT? UUT { get; set; }
    public string? LogDirectory { get; set; }
    public SshList? SshList { get; set; }
    public TelnetList? TelnetList { get; set; }
    public HttpList? HttpList { get; set; }
    public Variables? Variables { get; set; }
    public Constants? Constants { get; set; }
    public Inputs? Inputs { get; set; }
}

public class UUT
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? Prompt { get; set; }
}

public class SshList
{
    public List<SshConnection> Connections { get; set; } = new();
}

public class SshConnection
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Prompt { get; set; } = "$";
}

public class TelnetList
{
    public List<TelnetConnection> Connections { get; set; } = new();
}

public class TelnetConnection
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 23;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Prompt { get; set; } = ">";
}

public class HttpList
{
    public List<HttpConnection> Connections { get; set; } = new();
}

public class HttpConnection
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}

public class Variables
{
    public List<Variable> VariableList { get; set; } = new();
}

public class Variable
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Constants
{
    public List<Constant> ConstantList { get; set; } = new();
}

public class Constant
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Inputs
{
    public List<Input> InputList { get; set; } = new();
}

public class Input
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}