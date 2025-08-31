using System.Xml.Linq;
using SwpfEditor.Domain.Models;

namespace SwpfEditor.Infrastructure.Mapping;

public static class XmlTestMapper
{
    public static Test FromXElement(XElement element)
    {
        var test = new Test
        {
            Id = element.Attribute("id")?.Value ?? string.Empty,
            Alias = element.Attribute("alias")?.Value
        };

        // Map child elements
        var metaElement = element.Element("meta");
        if (metaElement != null)
        {
            test.Meta = MapMeta(metaElement);
        }

        test.Description = element.Element("description")?.Value;
        
        var displayOrderElement = element.Element("displayOrder");
        if (displayOrderElement != null && int.TryParse(displayOrderElement.Value, out var displayOrder))
        {
            test.DisplayOrder = displayOrder;
        }

        var sessionsElement = element.Element("sessions");
        if (sessionsElement != null)
        {
            test.Sessions = MapSessions(sessionsElement);
        }

        var configElement = element.Element("config");
        if (configElement != null)
        {
            test.Config = MapConfig(configElement);
        }

        var functionsElement = element.Element("functions");
        if (functionsElement != null)
        {
            test.Functions = MapFunctions(functionsElement);
        }

        var stepsElement = element.Element("steps");
        if (stepsElement != null)
        {
            test.Steps = MapSteps(stepsElement);
        }

        var testGroupsElement = element.Element("testGroups");
        if (testGroupsElement != null)
        {
            test.TestGroups = MapTestGroups(testGroupsElement);
        }

        var sectionsElement = element.Element("sections");
        if (sectionsElement != null)
        {
            test.Sections = MapSections(sectionsElement);
        }

        return test;
    }

    public static XElement ToXElement(Test test)
    {
        var element = new XElement("test");
        element.SetAttributeValue("id", test.Id);
        if (!string.IsNullOrEmpty(test.Alias))
            element.SetAttributeValue("alias", test.Alias);

        // Add child elements in order
        if (test.Meta != null)
            element.Add(ToXElement(test.Meta));

        if (!string.IsNullOrEmpty(test.Description))
            element.Add(new XElement("description", test.Description));

        if (test.DisplayOrder.HasValue)
            element.Add(new XElement("displayOrder", test.DisplayOrder.Value));

        if (test.Sessions != null)
            element.Add(ToXElement(test.Sessions));

        if (test.Config != null)
            element.Add(ToXElement(test.Config));

        if (test.Functions != null)
            element.Add(ToXElement(test.Functions));

        if (test.Steps != null)
            element.Add(ToXElement(test.Steps));

        if (test.TestGroups != null)
            element.Add(ToXElement(test.TestGroups));

        if (test.Sections != null)
            element.Add(ToXElement(test.Sections));

        return element;
    }

    private static Meta MapMeta(XElement element)
    {
        var meta = new Meta
        {
            Author = element.Element("author")?.Value,
            Version = element.Element("version")?.Value
        };

        var createdElement = element.Element("created");
        if (createdElement != null && DateTime.TryParse(createdElement.Value, out var created))
        {
            meta.Created = created;
        }

        var modifiedElement = element.Element("modified");
        if (modifiedElement != null && DateTime.TryParse(modifiedElement.Value, out var modified))
        {
            meta.Modified = modified;
        }

        return meta;
    }

    private static XElement ToXElement(Meta meta)
    {
        var element = new XElement("meta");
        
        if (!string.IsNullOrEmpty(meta.Author))
            element.Add(new XElement("author", meta.Author));
        
        if (!string.IsNullOrEmpty(meta.Version))
            element.Add(new XElement("version", meta.Version));
        
        if (meta.Created.HasValue)
            element.Add(new XElement("created", meta.Created.Value.ToString("O")));
        
        if (meta.Modified.HasValue)
            element.Add(new XElement("modified", meta.Modified.Value.ToString("O")));

        return element;
    }

    private static Sessions MapSessions(XElement element)
    {
        var sessions = new Sessions();
        
        foreach (var sessionElement in element.Elements("session"))
        {
            sessions.SessionList.Add(MapSession(sessionElement));
        }

        return sessions;
    }

    private static Session MapSession(XElement element)
    {
        var session = new Session
        {
            Name = element.Attribute("name")?.Value ?? string.Empty,
            Host = element.Attribute("host")?.Value,
            BaseUrl = element.Attribute("baseUrl")?.Value,
            User = element.Attribute("user")?.Value,
            Password = element.Attribute("password")?.Value,
            Prompt = element.Attribute("prompt")?.Value
        };

        var typeStr = element.Attribute("type")?.Value;
        if (Enum.TryParse<TargetType>(typeStr, true, out var targetType))
        {
            session.Type = targetType;
        }

        var portStr = element.Attribute("port")?.Value;
        if (int.TryParse(portStr, out var port))
        {
            session.Port = port;
        }

        return session;
    }

    private static XElement ToXElement(Sessions sessions)
    {
        var element = new XElement("sessions");
        
        foreach (var session in sessions.SessionList)
        {
            element.Add(ToXElement(session));
        }

        return element;
    }

    private static XElement ToXElement(Session session)
    {
        var element = new XElement("session");
        element.SetAttributeValue("name", session.Name);
        element.SetAttributeValue("type", session.Type.ToString().ToLowerInvariant());
        
        if (!string.IsNullOrEmpty(session.Host))
            element.SetAttributeValue("host", session.Host);
        
        if (session.Port.HasValue)
            element.SetAttributeValue("port", session.Port.Value);
        
        if (!string.IsNullOrEmpty(session.BaseUrl))
            element.SetAttributeValue("baseUrl", session.BaseUrl);
        
        if (!string.IsNullOrEmpty(session.User))
            element.SetAttributeValue("user", session.User);
        
        if (!string.IsNullOrEmpty(session.Password))
            element.SetAttributeValue("password", session.Password);
        
        if (!string.IsNullOrEmpty(session.Prompt))
            element.SetAttributeValue("prompt", session.Prompt);

        return element;
    }

    private static Config MapConfig(XElement element)
    {
        var config = new Config();
        
        foreach (var child in element.Elements())
        {
            config.Properties[child.Name.LocalName] = child.Value;
        }

        return config;
    }

    private static XElement ToXElement(Config config)
    {
        var element = new XElement("config");
        
        foreach (var property in config.Properties)
        {
            element.Add(new XElement(property.Key, property.Value));
        }

        return element;
    }

    private static Functions MapFunctions(XElement element)
    {
        var functions = new Functions();
        
        foreach (var child in element.Elements())
        {
            functions.Properties[child.Name.LocalName] = child.Value;
        }

        return functions;
    }

    private static XElement ToXElement(Functions functions)
    {
        var element = new XElement("functions");
        
        foreach (var property in functions.Properties)
        {
            element.Add(new XElement(property.Key, property.Value));
        }

        return element;
    }

    private static Steps MapSteps(XElement element)
    {
        var steps = new Steps();
        
        foreach (var stepElement in element.Elements("step"))
        {
            steps.StepList.Add(MapStep(stepElement));
        }

        return steps;
    }

    private static Step MapStep(XElement element)
    {
        var step = new Step
        {
            Id = element.Attribute("id")?.Value ?? string.Empty,
            Alias = element.Attribute("alias")?.Value,
            Target = element.Attribute("target")?.Value,
            Command = element.Attribute("command")?.Value
        };

        var targetTypeStr = element.Attribute("targetType")?.Value;
        if (Enum.TryParse<TargetType>(targetTypeStr, true, out var targetType))
        {
            step.TargetType = targetType;
        }

        var timeoutStr = element.Attribute("timeout")?.Value;
        if (int.TryParse(timeoutStr, out var timeout))
        {
            step.Timeout = timeout;
        }

        var methodStr = element.Attribute("method")?.Value;
        if (Enum.TryParse<Domain.Models.HttpMethod>(methodStr, true, out var method))
        {
            step.Method = method;
        }

        // Map child elements
        var paramsElement = element.Element("params");
        if (paramsElement != null)
        {
            step.Params = MapParams(paramsElement);
        }

        var headersElement = element.Element("headers");
        if (headersElement != null)
        {
            step.Headers = MapHeaders(headersElement);
        }

        var extractsElement = element.Element("extracts");
        if (extractsElement != null)
        {
            step.Extracts = MapExtracts(extractsElement);
        }

        var interactionElement = element.Element("interaction");
        if (interactionElement != null)
        {
            step.Interaction = MapInteraction(interactionElement);
        }

        return step;
    }

    private static XElement ToXElement(Steps steps)
    {
        var element = new XElement("steps");
        
        foreach (var step in steps.StepList)
        {
            element.Add(ToXElement(step));
        }

        return element;
    }

    private static XElement ToXElement(Step step)
    {
        var element = new XElement("step");
        element.SetAttributeValue("id", step.Id);
        
        if (!string.IsNullOrEmpty(step.Alias))
            element.SetAttributeValue("alias", step.Alias);
        
        if (!string.IsNullOrEmpty(step.Target))
            element.SetAttributeValue("target", step.Target);
        
        if (step.TargetType.HasValue)
            element.SetAttributeValue("targetType", step.TargetType.Value.ToString().ToLowerInvariant());
        
        if (step.Timeout.HasValue)
            element.SetAttributeValue("timeout", step.Timeout.Value);
        
        if (step.Method.HasValue)
            element.SetAttributeValue("method", step.Method.Value.ToString());
        
        if (!string.IsNullOrEmpty(step.Command))
            element.SetAttributeValue("command", step.Command);

        // Add child elements
        if (step.Params != null)
            element.Add(ToXElement(step.Params));
        
        if (step.Headers != null)
            element.Add(ToXElement(step.Headers));
        
        if (step.Extracts != null)
            element.Add(ToXElement(step.Extracts));
        
        if (step.Interaction != null)
            element.Add(ToXElement(step.Interaction));

        return element;
    }

    // Continue with remaining mapping methods...
    private static Params MapParams(XElement element)
    {
        var parameters = new Params();
        
        foreach (var paramElement in element.Elements("param"))
        {
            parameters.ParamList.Add(new Param
            {
                Name = paramElement.Attribute("name")?.Value ?? string.Empty,
                Value = paramElement.Attribute("value")?.Value ?? string.Empty
            });
        }

        return parameters;
    }

    private static XElement ToXElement(Params parameters)
    {
        var element = new XElement("params");
        
        foreach (var param in parameters.ParamList)
        {
            var paramElement = new XElement("param");
            paramElement.SetAttributeValue("name", param.Name);
            paramElement.SetAttributeValue("value", param.Value);
            element.Add(paramElement);
        }

        return element;
    }

    private static Headers MapHeaders(XElement element)
    {
        var headers = new Headers();
        
        foreach (var headerElement in element.Elements("header"))
        {
            headers.HeaderList.Add(new Header
            {
                Name = headerElement.Attribute("name")?.Value ?? string.Empty,
                Value = headerElement.Attribute("value")?.Value ?? string.Empty
            });
        }

        return headers;
    }

    private static XElement ToXElement(Headers headers)
    {
        var element = new XElement("headers");
        
        foreach (var header in headers.HeaderList)
        {
            var headerElement = new XElement("header");
            headerElement.SetAttributeValue("name", header.Name);
            headerElement.SetAttributeValue("value", header.Value);
            element.Add(headerElement);
        }

        return element;
    }

    private static Extracts MapExtracts(XElement element)
    {
        var extracts = new Extracts();
        
        foreach (var extractElement in element.Elements("extract"))
        {
            extracts.ExtractList.Add(MapExtract(extractElement));
        }

        return extracts;
    }

    private static Extract MapExtract(XElement element)
    {
        var extract = new Extract
        {
            Name = element.Attribute("name")?.Value ?? string.Empty,
            Pattern = element.Attribute("pattern")?.Value ?? string.Empty,
            Options = element.Attribute("options")?.Value
        };

        var checksElement = element.Element("checks");
        if (checksElement != null)
        {
            extract.Checks = MapChecks(checksElement);
        }

        return extract;
    }

    private static XElement ToXElement(Extracts extracts)
    {
        var element = new XElement("extracts");
        
        foreach (var extract in extracts.ExtractList)
        {
            element.Add(ToXElement(extract));
        }

        return element;
    }

    private static XElement ToXElement(Extract extract)
    {
        var element = new XElement("extract");
        element.SetAttributeValue("name", extract.Name);
        element.SetAttributeValue("pattern", extract.Pattern);
        
        if (!string.IsNullOrEmpty(extract.Options))
            element.SetAttributeValue("options", extract.Options);

        if (extract.Checks != null)
            element.Add(ToXElement(extract.Checks));

        return element;
    }

    private static Checks MapChecks(XElement element)
    {
        var checks = new Checks();
        
        foreach (var checkElement in element.Elements("check"))
        {
            checks.CheckList.Add(new Check
            {
                SourceRef = checkElement.Attribute("sourceRef")?.Value ?? string.Empty,
                Expect = checkElement.Attribute("expect")?.Value ?? string.Empty
            });
        }

        return checks;
    }

    private static XElement ToXElement(Checks checks)
    {
        var element = new XElement("checks");
        
        foreach (var check in checks.CheckList)
        {
            var checkElement = new XElement("check");
            checkElement.SetAttributeValue("sourceRef", check.SourceRef);
            checkElement.SetAttributeValue("expect", check.Expect);
            element.Add(checkElement);
        }

        return element;
    }

    private static Interaction MapInteraction(XElement element)
    {
        var interaction = new Interaction();
        
        foreach (var child in element.Elements())
        {
            interaction.Properties[child.Name.LocalName] = child.Value;
        }

        return interaction;
    }

    private static XElement ToXElement(Interaction interaction)
    {
        var element = new XElement("interaction");
        
        foreach (var property in interaction.Properties)
        {
            element.Add(new XElement(property.Key, property.Value));
        }

        return element;
    }

    private static TestGroups MapTestGroups(XElement element)
    {
        var testGroups = new TestGroups();
        
        foreach (var testGroupElement in element.Elements("testGroup"))
        {
            var testGroup = new TestGroup
            {
                Name = testGroupElement.Attribute("name")?.Value ?? string.Empty
            };

            foreach (var child in testGroupElement.Elements())
            {
                testGroup.Properties[child.Name.LocalName] = child.Value;
            }

            testGroups.TestGroupList.Add(testGroup);
        }

        return testGroups;
    }

    private static XElement ToXElement(TestGroups testGroups)
    {
        var element = new XElement("testGroups");
        
        foreach (var testGroup in testGroups.TestGroupList)
        {
            var testGroupElement = new XElement("testGroup");
            testGroupElement.SetAttributeValue("name", testGroup.Name);
            
            foreach (var property in testGroup.Properties)
            {
                testGroupElement.Add(new XElement(property.Key, property.Value));
            }
            
            element.Add(testGroupElement);
        }

        return element;
    }

    private static Sections MapSections(XElement element)
    {
        var sections = new Sections();
        
        foreach (var sectionElement in element.Elements("section"))
        {
            sections.SectionList.Add(MapSection(sectionElement));
        }

        return sections;
    }

    private static Section MapSection(XElement element)
    {
        var section = new Section
        {
            Id = element.Attribute("id")?.Value ?? string.Empty,
            Alias = element.Attribute("alias")?.Value
        };

        // Map steps directly under section
        foreach (var stepElement in element.Elements("step"))
        {
            section.Steps.Add(MapStep(stepElement));
        }

        // Map refs
        var refsElement = element.Element("refs");
        if (refsElement != null)
        {
            section.Refs = MapRefs(refsElement);
        }

        return section;
    }

    private static XElement ToXElement(Sections sections)
    {
        var element = new XElement("sections");
        
        foreach (var section in sections.SectionList)
        {
            element.Add(ToXElement(section));
        }

        return element;
    }

    private static XElement ToXElement(Section section)
    {
        var element = new XElement("section");
        element.SetAttributeValue("id", section.Id);
        
        if (!string.IsNullOrEmpty(section.Alias))
            element.SetAttributeValue("alias", section.Alias);

        // Add steps
        foreach (var step in section.Steps)
        {
            element.Add(ToXElement(step));
        }

        // Add refs
        if (section.Refs != null)
            element.Add(ToXElement(section.Refs));

        return element;
    }

    private static Refs MapRefs(XElement element)
    {
        var refs = new Refs();
        
        foreach (var refElement in element.Elements("ref"))
        {
            var refItem = new Ref
            {
                Step = refElement.Attribute("step")?.Value ?? string.Empty
            };

            var modeStr = refElement.Attribute("mode")?.Value;
            if (Enum.TryParse<RefMode>(modeStr, true, out var mode))
            {
                refItem.Mode = mode;
            }

            refs.RefList.Add(refItem);
        }

        return refs;
    }

    private static XElement ToXElement(Refs refs)
    {
        var element = new XElement("refs");
        
        foreach (var refItem in refs.RefList)
        {
            var refElement = new XElement("ref");
            refElement.SetAttributeValue("step", refItem.Step);
            
            if (refItem.Mode != RefMode.Id)
                refElement.SetAttributeValue("mode", refItem.Mode.ToString().ToLowerInvariant());
            
            element.Add(refElement);
        }

        return element;
    }
}