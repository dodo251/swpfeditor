using SwpfEditor.Domain.Enums;
using SwpfEditor.Domain.Models;
using System.Xml.Linq;

namespace SwpfEditor.Domain.Services;

/// <summary>
/// Maps between XML documents and POCO models
/// </summary>
public class XmlMappingService
{
    /// <summary>
    /// Convert Test POCO to XDocument
    /// </summary>
    public XDocument TestToXml(Test test)
    {
        var root = new XElement("test",
            new XAttribute("id", test.Id));

        if (!string.IsNullOrEmpty(test.Alias))
            root.Add(new XAttribute("alias", test.Alias));

        // Add meta if present
        if (test.Meta.Any())
        {
            var metaElement = new XElement("meta");
            foreach (var item in test.Meta)
            {
                metaElement.Add(new XElement(item.Key, item.Value));
            }
            root.Add(metaElement);
        }

        // Add description
        if (!string.IsNullOrEmpty(test.Description))
            root.Add(new XElement("description", test.Description));

        // Add display order
        if (!string.IsNullOrEmpty(test.DisplayOrder))
            root.Add(new XElement("displayOrder", test.DisplayOrder));

        // Add sessions
        if (test.Sessions.Any())
        {
            var sessionsElement = new XElement("sessions");
            foreach (var session in test.Sessions)
            {
                sessionsElement.Add(SessionToXml(session));
            }
            root.Add(sessionsElement);
        }

        // Add config if present
        if (test.Config.Any())
        {
            var configElement = new XElement("config");
            foreach (var item in test.Config)
            {
                configElement.Add(new XElement(item.Key, item.Value));
            }
            root.Add(configElement);
        }

        // Add functions if present
        if (test.Functions.Any())
        {
            var functionsElement = new XElement("functions");
            foreach (var item in test.Functions)
            {
                functionsElement.Add(new XElement(item.Key, item.Value));
            }
            root.Add(functionsElement);
        }

        // Add steps
        if (test.Steps.Any())
        {
            var stepsElement = new XElement("steps");
            foreach (var step in test.Steps)
            {
                stepsElement.Add(StepToXml(step));
            }
            root.Add(stepsElement);
        }

        // Add test groups if present
        if (test.TestGroups.Any())
        {
            var testGroupsElement = new XElement("testGroups");
            foreach (var item in test.TestGroups)
            {
                testGroupsElement.Add(new XElement(item.Key, item.Value));
            }
            root.Add(testGroupsElement);
        }

        // Add sections
        if (test.Sections.Any())
        {
            var sectionsElement = new XElement("sections");
            foreach (var section in test.Sections)
            {
                sectionsElement.Add(SectionToXml(section));
            }
            root.Add(sectionsElement);
        }

        return new XDocument(root);
    }

    /// <summary>
    /// Convert XDocument to Test POCO
    /// </summary>
    public Test XmlToTest(XDocument document)
    {
        var root = document.Root ?? throw new ArgumentException("Document has no root element");
        
        var test = new Test
        {
            Id = root.Attribute("id")?.Value ?? "",
            Alias = root.Attribute("alias")?.Value
        };

        // Parse meta
        var metaElement = root.Element("meta");
        if (metaElement != null)
        {
            foreach (var child in metaElement.Elements())
            {
                test.Meta[child.Name.LocalName] = child.Value;
            }
        }

        // Parse description
        test.Description = root.Element("description")?.Value;
        test.DisplayOrder = root.Element("displayOrder")?.Value;

        // Parse sessions
        var sessionsElement = root.Element("sessions");
        if (sessionsElement != null)
        {
            foreach (var sessionElement in sessionsElement.Elements("session"))
            {
                test.Sessions.Add(XmlToSession(sessionElement));
            }
        }

        // Parse config
        var configElement = root.Element("config");
        if (configElement != null)
        {
            foreach (var child in configElement.Elements())
            {
                test.Config[child.Name.LocalName] = child.Value;
            }
        }

        // Parse functions
        var functionsElement = root.Element("functions");
        if (functionsElement != null)
        {
            foreach (var child in functionsElement.Elements())
            {
                test.Functions[child.Name.LocalName] = child.Value;
            }
        }

        // Parse steps
        var stepsElement = root.Element("steps");
        if (stepsElement != null)
        {
            foreach (var stepElement in stepsElement.Elements("step"))
            {
                test.Steps.Add(XmlToStep(stepElement));
            }
        }

        // Parse test groups
        var testGroupsElement = root.Element("testGroups");
        if (testGroupsElement != null)
        {
            foreach (var child in testGroupsElement.Elements())
            {
                test.TestGroups[child.Name.LocalName] = child.Value;
            }
        }

        // Parse sections
        var sectionsElement = root.Element("sections");
        if (sectionsElement != null)
        {
            foreach (var sectionElement in sectionsElement.Elements("section"))
            {
                test.Sections.Add(XmlToSection(sectionElement));
            }
        }

        return test;
    }

    private XElement SessionToXml(Session session)
    {
        var element = new XElement("session",
            new XAttribute("name", session.Name),
            new XAttribute("type", session.Type.ToString().ToLowerInvariant()));

        if (!string.IsNullOrEmpty(session.Host))
            element.Add(new XAttribute("host", session.Host));
        if (session.Port.HasValue)
            element.Add(new XAttribute("port", session.Port.Value));
        if (!string.IsNullOrEmpty(session.BaseUrl))
            element.Add(new XAttribute("baseUrl", session.BaseUrl));
        if (!string.IsNullOrEmpty(session.User))
            element.Add(new XAttribute("user", session.User));
        if (!string.IsNullOrEmpty(session.Password))
            element.Add(new XAttribute("password", session.Password));
        if (!string.IsNullOrEmpty(session.Prompt))
            element.Add(new XAttribute("prompt", session.Prompt));

        return element;
    }

    private Session XmlToSession(XElement element)
    {
        return new Session
        {
            Name = element.Attribute("name")?.Value ?? "",
            Type = ParseTargetType(element.Attribute("type")?.Value ?? ""),
            Host = element.Attribute("host")?.Value,
            Port = int.TryParse(element.Attribute("port")?.Value, out var port) ? port : null,
            BaseUrl = element.Attribute("baseUrl")?.Value,
            User = element.Attribute("user")?.Value,
            Password = element.Attribute("password")?.Value,
            Prompt = element.Attribute("prompt")?.Value
        };
    }

    private XElement StepToXml(Step step)
    {
        var element = new XElement("step",
            new XAttribute("id", step.Id),
            new XAttribute("target", step.Target),
            new XAttribute("targetType", step.TargetType.ToString().ToLowerInvariant()),
            new XAttribute("timeout", step.Timeout));

        if (!string.IsNullOrEmpty(step.Alias))
            element.Add(new XAttribute("alias", step.Alias));
        if (step.Method.HasValue)
            element.Add(new XAttribute("method", step.Method.Value.ToString()));
        if (!string.IsNullOrEmpty(step.Command))
            element.Add(new XAttribute("command", step.Command));

        // Add params
        if (step.Params.Any())
        {
            var paramsElement = new XElement("params");
            foreach (var param in step.Params)
            {
                paramsElement.Add(new XElement(param.Key, param.Value));
            }
            element.Add(paramsElement);
        }

        // Add headers
        if (step.Headers.Any())
        {
            var headersElement = new XElement("headers");
            foreach (var header in step.Headers)
            {
                headersElement.Add(new XElement(header.Key, header.Value));
            }
            element.Add(headersElement);
        }

        // Add extracts
        if (step.Extracts.Any())
        {
            var extractsElement = new XElement("extracts");
            foreach (var extract in step.Extracts)
            {
                extractsElement.Add(ExtractToXml(extract));
            }
            element.Add(extractsElement);
        }

        // Add interaction
        if (step.Interaction.Any())
        {
            var interactionElement = new XElement("interaction");
            foreach (var interaction in step.Interaction)
            {
                interactionElement.Add(new XElement(interaction.Key, interaction.Value));
            }
            element.Add(interactionElement);
        }

        return element;
    }

    private Step XmlToStep(XElement element)
    {
        var step = new Step
        {
            Id = element.Attribute("id")?.Value ?? "",
            Alias = element.Attribute("alias")?.Value,
            Target = element.Attribute("target")?.Value ?? "",
            TargetType = ParseTargetType(element.Attribute("targetType")?.Value ?? ""),
            Timeout = int.TryParse(element.Attribute("timeout")?.Value, out var timeout) ? timeout : 0,
            Method = ParseHttpMethod(element.Attribute("method")?.Value),
            Command = element.Attribute("command")?.Value
        };

        // Parse params
        var paramsElement = element.Element("params");
        if (paramsElement != null)
        {
            foreach (var child in paramsElement.Elements())
            {
                step.Params[child.Name.LocalName] = child.Value;
            }
        }

        // Parse headers
        var headersElement = element.Element("headers");
        if (headersElement != null)
        {
            foreach (var child in headersElement.Elements())
            {
                step.Headers[child.Name.LocalName] = child.Value;
            }
        }

        // Parse extracts
        var extractsElement = element.Element("extracts");
        if (extractsElement != null)
        {
            foreach (var extractElement in extractsElement.Elements("extract"))
            {
                step.Extracts.Add(XmlToExtract(extractElement));
            }
        }

        // Parse interaction
        var interactionElement = element.Element("interaction");
        if (interactionElement != null)
        {
            foreach (var child in interactionElement.Elements())
            {
                step.Interaction[child.Name.LocalName] = child.Value;
            }
        }

        return step;
    }

    private XElement ExtractToXml(Extract extract)
    {
        var element = new XElement("extract",
            new XAttribute("name", extract.Name),
            new XAttribute("pattern", extract.Pattern));

        if (!string.IsNullOrEmpty(extract.Options))
            element.Add(new XAttribute("options", extract.Options));

        if (extract.Checks.Any())
        {
            var checksElement = new XElement("checks");
            foreach (var check in extract.Checks)
            {
                checksElement.Add(new XElement("check",
                    new XAttribute("expect", check.Expect),
                    new XAttribute("sourceRef", check.SourceRef)));
            }
            element.Add(checksElement);
        }

        return element;
    }

    private Extract XmlToExtract(XElement element)
    {
        var extract = new Extract
        {
            Name = element.Attribute("name")?.Value ?? "",
            Pattern = element.Attribute("pattern")?.Value ?? "",
            Options = element.Attribute("options")?.Value
        };

        var checksElement = element.Element("checks");
        if (checksElement != null)
        {
            foreach (var checkElement in checksElement.Elements("check"))
            {
                extract.Checks.Add(new Check
                {
                    Expect = checkElement.Attribute("expect")?.Value ?? "",
                    SourceRef = checkElement.Attribute("sourceRef")?.Value ?? ""
                });
            }
        }

        return extract;
    }

    private XElement SectionToXml(Section section)
    {
        var element = new XElement("section",
            new XAttribute("id", section.Id));

        if (!string.IsNullOrEmpty(section.Name))
            element.Add(new XAttribute("name", section.Name));
        if (section.Type != SectionType.Serial)
            element.Add(new XAttribute("type", section.Type.ToString().ToLowerInvariant()));
        if (section.FailContinue)
            element.Add(new XAttribute("failContinue", section.FailContinue));
        if (section.RetryCount > 0)
            element.Add(new XAttribute("retryCount", section.RetryCount));
        if (!string.IsNullOrEmpty(section.RetestPoint))
            element.Add(new XAttribute("retestPoint", section.RetestPoint));
        if (!string.IsNullOrEmpty(section.PassNext))
            element.Add(new XAttribute("passNext", section.PassNext));
        if (!string.IsNullOrEmpty(section.FailNext))
            element.Add(new XAttribute("failNext", section.FailNext));

        if (section.Refs.Any())
        {
            var refsElement = new XElement("refs");
            foreach (var stepRef in section.Refs)
            {
                var refElement = new XElement("ref",
                    new XAttribute("step", stepRef.Step));
                if (stepRef.Mode != RefMode.Id)
                    refElement.Add(new XAttribute("mode", stepRef.Mode.ToString().ToLowerInvariant()));
                refsElement.Add(refElement);
            }
            element.Add(refsElement);
        }

        return element;
    }

    private Section XmlToSection(XElement element)
    {
        var section = new Section
        {
            Id = element.Attribute("id")?.Value ?? "",
            Name = element.Attribute("name")?.Value,
            Type = ParseSectionType(element.Attribute("type")?.Value ?? "serial"),
            FailContinue = bool.TryParse(element.Attribute("failContinue")?.Value, out var failContinue) && failContinue,
            RetryCount = int.TryParse(element.Attribute("retryCount")?.Value, out var retryCount) ? retryCount : 0,
            RetestPoint = element.Attribute("retestPoint")?.Value,
            PassNext = element.Attribute("passNext")?.Value,
            FailNext = element.Attribute("failNext")?.Value
        };

        var refsElement = element.Element("refs");
        if (refsElement != null)
        {
            foreach (var refElement in refsElement.Elements("ref"))
            {
                section.Refs.Add(new Ref
                {
                    Step = refElement.Attribute("step")?.Value ?? "",
                    Mode = ParseRefMode(refElement.Attribute("mode")?.Value ?? "id")
                });
            }
        }

        return section;
    }

    private TargetType ParseTargetType(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "ssh" => TargetType.Ssh,
            "http" => TargetType.Http,
            "telnet" => TargetType.Telnet,
            "manual" => TargetType.Manual,
            _ => TargetType.Manual
        };
    }

    private Enums.HttpMethod? ParseHttpMethod(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return value.ToUpperInvariant() switch
        {
            "GET" => Enums.HttpMethod.GET,
            "POST" => Enums.HttpMethod.POST,
            "PUT" => Enums.HttpMethod.PUT,
            "DELETE" => Enums.HttpMethod.DELETE,
            "HEAD" => Enums.HttpMethod.HEAD,
            "PATCH" => Enums.HttpMethod.PATCH,
            _ => null
        };
    }

    private SectionType ParseSectionType(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "serial" => SectionType.Serial,
            "manual" => SectionType.Manual,
            "final" => SectionType.Final,
            "cleanup" => SectionType.Cleanup,
            _ => SectionType.Serial
        };
    }

    private RefMode ParseRefMode(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "id" => RefMode.Id,
            "alias" => RefMode.Alias,
            _ => RefMode.Id
        };
    }
}