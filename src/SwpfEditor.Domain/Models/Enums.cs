namespace SwpfEditor.Domain.Models;

public enum TargetType
{
    Ssh,
    Http,
    Telnet,
    Manual
}

public enum HttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    PATCH,
    HEAD,
    OPTIONS
}

public enum RefMode
{
    Id,
    Alias
}