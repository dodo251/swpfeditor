namespace SwpfEditor.Domain.Enums;

/// <summary>
/// Target type for steps and sessions
/// </summary>
public enum TargetType
{
    Ssh,
    Http,
    Telnet,
    Manual
}

/// <summary>
/// HTTP methods for HTTP-based steps
/// </summary>
public enum HttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    HEAD,
    PATCH
}

/// <summary>
/// Section types
/// </summary>
public enum SectionType
{
    Serial,
    Manual,
    Final,
    Cleanup
}

/// <summary>
/// Reference mode for section refs
/// </summary>
public enum RefMode
{
    Id,
    Alias
}

/// <summary>
/// Validation severity levels
/// </summary>
public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}