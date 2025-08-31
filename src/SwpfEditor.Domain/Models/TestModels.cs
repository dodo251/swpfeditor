using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Models;

/// <summary>
/// Check validation for extracted values
/// </summary>
public class Check
{
    public string Expect { get; set; } = string.Empty;
    public string SourceRef { get; set; } = string.Empty;
}

/// <summary>
/// Extract values from step execution
/// </summary>
public class Extract
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string? Options { get; set; }
    public List<Check> Checks { get; set; } = new();
}

/// <summary>
/// Test step execution unit
/// </summary>
public class Step
{
    public string Id { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string Target { get; set; } = string.Empty;
    public TargetType TargetType { get; set; }
    public int Timeout { get; set; }
    public Enums.HttpMethod? Method { get; set; }
    public string? Command { get; set; }
    
    public List<Extract> Extracts { get; set; } = new();
    public Dictionary<string, string> Params { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, object> Interaction { get; set; } = new();
}

/// <summary>
/// Reference to a step within a section
/// </summary>
public class Ref
{
    public string Step { get; set; } = string.Empty;
    public RefMode Mode { get; set; } = RefMode.Id;
}

/// <summary>
/// Test section grouping steps
/// </summary>
public class Section
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public SectionType Type { get; set; } = SectionType.Serial;
    public bool FailContinue { get; set; }
    public int RetryCount { get; set; }
    public string? RetestPoint { get; set; }
    public string? PassNext { get; set; }
    public string? FailNext { get; set; }
    
    public List<Ref> Refs { get; set; } = new();
}

/// <summary>
/// Session configuration for connections
/// </summary>
public class Session
{
    public string Name { get; set; } = string.Empty;
    public TargetType Type { get; set; }
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? BaseUrl { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? Prompt { get; set; }
}

/// <summary>
/// Root test configuration
/// </summary>
public class Test
{
    public string Id { get; set; } = string.Empty;
    public string? Alias { get; set; }
    
    public Dictionary<string, object> Meta { get; set; } = new();
    public string? Description { get; set; }
    public string? DisplayOrder { get; set; }
    public List<Session> Sessions { get; set; } = new();
    public Dictionary<string, object> Config { get; set; } = new();
    public Dictionary<string, object> Functions { get; set; } = new();
    public List<Step> Steps { get; set; } = new();
    public Dictionary<string, object> TestGroups { get; set; } = new();
    public List<Section> Sections { get; set; } = new();
}