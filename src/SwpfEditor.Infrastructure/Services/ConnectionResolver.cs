using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Services;

namespace SwpfEditor.Infrastructure.Services;

public class ConnectionResolver : IConnectionResolver
{
    public ConnectionDetails? ResolveConnection(Step step, TestConfiguration testConfiguration)
    {
        if (step.TargetType == null || string.IsNullOrEmpty(step.Target))
            return null;

        return step.TargetType switch
        {
            Domain.Models.TargetType.Ssh => ResolveSshConnection(step.Target, testConfiguration),
            Domain.Models.TargetType.Http => ResolveHttpConnection(step.Target, testConfiguration),
            Domain.Models.TargetType.Telnet => ResolveTelnetConnection(step.Target, testConfiguration),
            Domain.Models.TargetType.Manual => new ConnectionDetails { Name = step.Target, Type = Domain.Models.TargetType.Manual },
            _ => null
        };
    }

    public Sessions GenerateSessionMappings(Test test, TestConfiguration testConfiguration)
    {
        var sessions = new Sessions();
        var usedConnections = new HashSet<string>();

        // Collect all unique target connections from steps
        CollectConnectionsFromSteps(test.Steps?.StepList, usedConnections);
        CollectConnectionsFromSections(test.Sections?.SectionList, usedConnections);

        // Generate session for each used connection
        foreach (var target in usedConnections)
        {
            var session = CreateSessionFromTarget(target, testConfiguration);
            if (session != null)
            {
                sessions.SessionList.Add(session);
            }
        }

        return sessions;
    }

    public List<ValidationError> ValidateConnections(Test test, TestConfiguration testConfiguration)
    {
        var errors = new List<ValidationError>();
        var allSteps = new List<Step>();

        // Collect all steps
        if (test.Steps?.StepList != null)
            allSteps.AddRange(test.Steps.StepList);

        if (test.Sections?.SectionList != null)
        {
            foreach (var section in test.Sections.SectionList)
            {
                allSteps.AddRange(section.Steps);
            }
        }

        // Validate each step's connection
        foreach (var step in allSteps)
        {
            if (step.TargetType != null && !string.IsNullOrEmpty(step.Target))
            {
                var connection = ResolveConnection(step, testConfiguration);
                if (connection == null)
                {
                    errors.Add(new ValidationError
                    {
                        Message = $"Cannot resolve connection '{step.Target}' of type '{step.TargetType}' for step '{step.Id}'",
                        ElementId = step.Id,
                        PropertyName = "target",
                        Severity = ValidationSeverity.Error
                    });
                }
            }
        }

        return errors;
    }

    private ConnectionDetails? ResolveSshConnection(string target, TestConfiguration testConfiguration)
    {
        // First check direct UUT match
        if (testConfiguration.UUT?.Name == target && testConfiguration.UUT.Type.Equals("ssh", StringComparison.OrdinalIgnoreCase))
        {
            return new ConnectionDetails
            {
                Name = testConfiguration.UUT.Name,
                Type = Domain.Models.TargetType.Ssh,
                Host = testConfiguration.UUT.Host,
                Port = testConfiguration.UUT.Port,
                User = testConfiguration.UUT.User,
                Password = testConfiguration.UUT.Password,
                Prompt = testConfiguration.UUT.Prompt
            };
        }

        // Check SSH list
        var sshConnection = testConfiguration.SshList?.Connections?.FirstOrDefault(s => s.Name == target);
        if (sshConnection != null)
        {
            return new ConnectionDetails
            {
                Name = sshConnection.Name,
                Type = Domain.Models.TargetType.Ssh,
                Host = sshConnection.Host,
                Port = sshConnection.Port,
                User = sshConnection.User,
                Password = sshConnection.Password,
                Prompt = sshConnection.Prompt
            };
        }

        return null;
    }

    private ConnectionDetails? ResolveHttpConnection(string target, TestConfiguration testConfiguration)
    {
        var httpConnection = testConfiguration.HttpList?.Connections?.FirstOrDefault(h => h.Name == target);
        if (httpConnection != null)
        {
            return new ConnectionDetails
            {
                Name = httpConnection.Name,
                Type = Domain.Models.TargetType.Http,
                BaseUrl = httpConnection.BaseUrl
            };
        }

        return null;
    }

    private ConnectionDetails? ResolveTelnetConnection(string target, TestConfiguration testConfiguration)
    {
        var telnetConnection = testConfiguration.TelnetList?.Connections?.FirstOrDefault(t => t.Name == target);
        if (telnetConnection != null)
        {
            return new ConnectionDetails
            {
                Name = telnetConnection.Name,
                Type = Domain.Models.TargetType.Telnet,
                Host = telnetConnection.Host,
                Port = telnetConnection.Port,
                User = telnetConnection.User,
                Password = telnetConnection.Password,
                Prompt = telnetConnection.Prompt
            };
        }

        return null;
    }

    private void CollectConnectionsFromSteps(List<Step>? steps, HashSet<string> usedConnections)
    {
        if (steps == null) return;

        foreach (var step in steps)
        {
            if (!string.IsNullOrEmpty(step.Target))
            {
                usedConnections.Add(step.Target);
            }
        }
    }

    private void CollectConnectionsFromSections(List<Section>? sections, HashSet<string> usedConnections)
    {
        if (sections == null) return;

        foreach (var section in sections)
        {
            CollectConnectionsFromSteps(section.Steps, usedConnections);
        }
    }

    private Session? CreateSessionFromTarget(string target, TestConfiguration testConfiguration)
    {
        // Try SSH first
        var sshConnection = testConfiguration.SshList?.Connections?.FirstOrDefault(s => s.Name == target);
        if (sshConnection != null)
        {
            return new Session
            {
                Name = sshConnection.Name,
                Type = Domain.Models.TargetType.Ssh,
                Host = sshConnection.Host,
                Port = sshConnection.Port,
                User = sshConnection.User,
                Password = sshConnection.Password,
                Prompt = sshConnection.Prompt
            };
        }

        // Try HTTP
        var httpConnection = testConfiguration.HttpList?.Connections?.FirstOrDefault(h => h.Name == target);
        if (httpConnection != null)
        {
            return new Session
            {
                Name = httpConnection.Name,
                Type = Domain.Models.TargetType.Http,
                BaseUrl = httpConnection.BaseUrl
            };
        }

        // Try Telnet
        var telnetConnection = testConfiguration.TelnetList?.Connections?.FirstOrDefault(t => t.Name == target);
        if (telnetConnection != null)
        {
            return new Session
            {
                Name = telnetConnection.Name,
                Type = Domain.Models.TargetType.Telnet,
                Host = telnetConnection.Host,
                Port = telnetConnection.Port,
                User = telnetConnection.User,
                Password = telnetConnection.Password,
                Prompt = telnetConnection.Prompt
            };
        }

        // Check UUT
        if (testConfiguration.UUT?.Name == target)
        {
            var targetType = testConfiguration.UUT.Type.ToLowerInvariant() switch
            {
                "ssh" => Domain.Models.TargetType.Ssh,
                "http" => Domain.Models.TargetType.Http,
                "telnet" => Domain.Models.TargetType.Telnet,
                _ => Domain.Models.TargetType.Manual
            };

            return new Session
            {
                Name = testConfiguration.UUT.Name,
                Type = targetType,
                Host = testConfiguration.UUT.Host,
                Port = testConfiguration.UUT.Port,
                User = testConfiguration.UUT.User,
                Password = testConfiguration.UUT.Password,
                Prompt = testConfiguration.UUT.Prompt
            };
        }

        return null;
    }
}