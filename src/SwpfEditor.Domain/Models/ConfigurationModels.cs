using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Models;

/// <summary>
/// SSH connection configuration
/// </summary>
public class SshConnection
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string? Prompt { get; set; }
}

/// <summary>
/// HTTP connection configuration
/// </summary>
public class HttpConnection
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int? Timeout { get; set; }
}

/// <summary>
/// Telnet connection configuration
/// </summary>
public class TelnetConnection
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Prompt { get; set; }
}

/// <summary>
/// Variable definition for placeholders
/// </summary>
public class Variable
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Constant definition for placeholders
/// </summary>
public class Constant
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Input definition for placeholders
/// </summary>
public class Input
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
}

/// <summary>
/// Unit Under Test configuration
/// </summary>
public class UUT
{
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Test configuration root
/// </summary>
public class TestConfiguration
{
    public UUT? UUT { get; set; }
    public string? LogDirectory { get; set; }
    public List<SshConnection> SshList { get; set; } = new();
    public List<TelnetConnection> TelnetList { get; set; } = new();
    public List<HttpConnection> HttpList { get; set; } = new();
    public List<Variable> Variables { get; set; } = new();
    public List<Constant> Constants { get; set; } = new();
    public List<Input> Inputs { get; set; } = new();
}