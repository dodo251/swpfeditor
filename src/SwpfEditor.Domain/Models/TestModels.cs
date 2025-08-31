using SwpfEditor.Domain.Enums;

namespace SwpfEditor.Domain.Models;

/// <summary>
/// Root test configuration
/// </summary>
public class Test
{
    public string Id { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public Meta? Meta { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public Sessions? Sessions { get; set; }
    public Config? Config { get; set; }
    public Functions? Functions { get; set; }
    public Steps? Steps { get; set; }
    public TestGroups? TestGroups { get; set; }
    public Sections? Sections { get; set; }
    
    // Alternative simplified structure for cross-platform compatibility
    public List<Step> StepsList { get; set; } = new();
    public List<Section> SectionsList { get; set; } = new();
}

public class Meta
{
    public string? Author { get; set; }
    public string? Version { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Modified { get; set; }
}

public class Sessions
{
    public List<Session> SessionList { get; set; } = new();
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

public class Config
{
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class Functions
{
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class Steps
{
    public List<Step> StepList { get; set; } = new();
}

/// <summary>
/// Test step execution unit
/// </summary>
public class Step
{
    public string Id { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string? Target { get; set; }
    public TargetType? TargetType { get; set; }
    public int? Timeout { get; set; }
    public Enums.HttpMethod? Method { get; set; }
    public string? Command { get; set; }
    public Params? Params { get; set; }
    public Headers? Headers { get; set; }
    public Extracts? Extracts { get; set; }
    public Interaction? Interaction { get; set; }
    
    // Alternative simplified structure for cross-platform compatibility
    public List<Extract> ExtractsList { get; set; } = new();
    public Dictionary<string, string> ParamsDict { get; set; } = new();
    public Dictionary<string, string> HeadersDict { get; set; } = new();
    public Dictionary<string, object> InteractionDict { get; set; } = new();
}

public class Params
{
    public List<Param> ParamList { get; set; } = new();
}

public class Param
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Headers
{
    public List<Header> HeaderList { get; set; } = new();
}

public class Header
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Extracts
{
    public List<Extract> ExtractList { get; set; } = new();
}

/// <summary>
/// Extract values from step execution
/// </summary>
public class Extract
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string? Options { get; set; }
    public Checks? Checks { get; set; }
    
    // Alternative simplified structure for cross-platform compatibility
    public List<Check> ChecksList { get; set; } = new();
}

public class Checks
{
    public List<Check> CheckList { get; set; } = new();
}

/// <summary>
/// Check validation for extracted values
/// </summary>
public class Check
{
    public string SourceRef { get; set; } = string.Empty;
    public string Expect { get; set; } = string.Empty;
}

public class Interaction
{
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class TestGroups
{
    public List<TestGroup> TestGroupList { get; set; } = new();
}

public class TestGroup
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class Sections
{
    public List<Section> SectionList { get; set; } = new();
}

/// <summary>
/// Test section grouping steps
/// </summary>
public class Section
{
    public string Id { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string? Name { get; set; }
    public SectionType Type { get; set; } = SectionType.Serial;
    public bool FailContinue { get; set; }
    public int RetryCount { get; set; }
    public string? RetestPoint { get; set; }
    public string? PassNext { get; set; }
    public string? FailNext { get; set; }
    public List<Step> Steps { get; set; } = new();
    public Refs? Refs { get; set; }
    
    // Alternative simplified structure for cross-platform compatibility  
    public List<Ref> RefsList { get; set; } = new();
}

public class Refs
{
    public List<Ref> RefList { get; set; } = new();
}

/// <summary>
/// Reference to a step within a section
/// </summary>
public class Ref
{
    public string Step { get; set; } = string.Empty;
    public RefMode Mode { get; set; } = RefMode.Id;
}