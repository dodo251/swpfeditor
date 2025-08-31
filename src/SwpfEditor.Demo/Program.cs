using SwpfEditor.Domain.Services;
using SwpfEditor.Domain.Models;
using SwpfEditor.Domain.Enums;
using System.Xml.Linq;

namespace SwpfEditor.Demo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== SwpfEditor Domain Demo ===");
        Console.WriteLine();
        
        // Initialize services
        var xmlMappingService = new XmlMappingService();
        var configMappingService = new ConfigurationMappingService();
        var placeholderResolver = new PlaceholderResolver();
        var dualXmlCoordinator = new DualXmlCoordinator(placeholderResolver);
        var validationService = new ValidationService(dualXmlCoordinator);

        // Paths to sample files
        var basePath = "/home/runner/work/swpfeditor/swpfeditor";
        var testPath = Path.Combine(basePath, "samples", "test.xml");
        var configPath = Path.Combine(basePath, "samples", "TestConfiguration.xml");
        var schemaPath = Path.Combine(basePath, "schemas", "test-schema-1.0.xsd");

        try
        {
            // Load and parse sample files
            Console.WriteLine("1. Loading sample files...");
            var testDocument = XDocument.Load(testPath);
            var configDocument = XDocument.Load(configPath);
            
            var test = xmlMappingService.XmlToTest(testDocument);
            var config = configMappingService.XmlToConfiguration(configDocument);
            
            Console.WriteLine($"   ✓ Loaded test '{test.Id}' ({test.Alias})");
            Console.WriteLine($"   ✓ Loaded configuration with {config.SshList.Count} SSH connections");
            Console.WriteLine($"   ✓ Test has {test.Steps.Count} steps:");
            foreach (var step in test.Steps)
            {
                Console.WriteLine($"      - Step {step.Id} (alias: {step.Alias ?? "none"})");
            }
            Console.WriteLine();

            // Validate against XSD
            Console.WriteLine("2. XSD Validation...");
            var xsdResults = validationService.ValidateAgainstXsd(testDocument, schemaPath);
            if (xsdResults.IsValid)
            {
                Console.WriteLine("   ✓ XSD validation passed");
            }
            else
            {
                Console.WriteLine("   ✗ XSD validation failed:");
                foreach (var error in xsdResults.Items.Where(i => i.Severity == ValidationSeverity.Error))
                {
                    Console.WriteLine($"     - {error.Message}");
                }
            }
            Console.WriteLine();

            // Initialize placeholder resolver
            Console.WriteLine("3. Initializing placeholder resolver...");
            dualXmlCoordinator.InitializePlaceholderResolver(config);
            Console.WriteLine($"   ✓ Loaded {config.Constants.Count} constants, {config.Variables.Count} variables, {config.Inputs.Count} inputs");
            Console.WriteLine();

            // Generate sessions from configuration
            Console.WriteLine("4. Generating sessions from configuration...");
            var sessions = dualXmlCoordinator.GenerateSessionsFromConfiguration(test, config);
            Console.WriteLine($"   ✓ Generated {sessions.Count} sessions:");
            foreach (var session in sessions)
            {
                Console.WriteLine($"     - {session.Name} ({session.Type})");
            }
            Console.WriteLine();

            // Validate step target mappings
            Console.WriteLine("5. Validating step target mappings...");
            var targetMappingErrors = dualXmlCoordinator.ValidateStepTargetMappings(test, config);
            if (targetMappingErrors.Any())
            {
                Console.WriteLine("   ✗ Target mapping errors:");
                foreach (var error in targetMappingErrors)
                {
                    Console.WriteLine($"     - {error}");
                }
            }
            else
            {
                Console.WriteLine("   ✓ All step targets mapped correctly");
            }
            Console.WriteLine();

            // Business rules validation
            Console.WriteLine("6. Running business rules validation...");
            var validationResults = validationService.ValidateTest(test, config);
            
            Console.WriteLine($"   Total validation items: {validationResults.Items.Count}");
            Console.WriteLine($"   Errors: {validationResults.Items.Count(i => i.Severity == ValidationSeverity.Error)}");
            Console.WriteLine($"   Warnings: {validationResults.Items.Count(i => i.Severity == ValidationSeverity.Warning)}");
            Console.WriteLine($"   Info: {validationResults.Items.Count(i => i.Severity == ValidationSeverity.Info)}");
            
            if (validationResults.HasErrors)
            {
                Console.WriteLine("   ✗ Validation failed with errors:");
                foreach (var error in validationResults.Items.Where(i => i.Severity == ValidationSeverity.Error))
                {
                    Console.WriteLine($"     - {error.Message}");
                    if (!string.IsNullOrEmpty(error.SuggestedFix))
                    {
                        Console.WriteLine($"       Suggestion: {error.SuggestedFix}");
                    }
                }
            }
            else
            {
                Console.WriteLine("   ✓ All business rules validation passed");
            }
            Console.WriteLine();

            // Demonstrate acceptance criteria
            Console.WriteLine("7. Testing acceptance criteria...");
            
            // Test GetOsViaSsh scenario
            var getOsStep = test.Steps.FirstOrDefault(s => s.Alias == "GetOsViaSsh");
            if (getOsStep != null)
            {
                Console.WriteLine($"   ✓ Found GetOsViaSsh step (ID: {getOsStep.Id})");
                Console.WriteLine($"   ✓ Target: {getOsStep.Target} ({getOsStep.TargetType})");
                Console.WriteLine($"   ✓ Command: {getOsStep.Command}");
                
                var dutSshSession = sessions.FirstOrDefault(s => s.Name == "DUT_SSH");
                if (dutSshSession != null)
                {
                    Console.WriteLine($"   ✓ DUT_SSH session mapped: {dutSshSession.Host}:{dutSshSession.Port}");
                    Console.WriteLine($"   ✓ SSH credentials: {dutSshSession.User}@{dutSshSession.Host}");
                }
                
                var osExtract = getOsStep.Extracts.FirstOrDefault(e => e.Name == "os");
                if (osExtract != null)
                {
                    Console.WriteLine($"   ✓ OS extract found with pattern: {osExtract.Pattern}");
                    var ubuntuCheck = osExtract.Checks.FirstOrDefault(c => c.Expect == "Ubuntu");
                    if (ubuntuCheck != null)
                    {
                        Console.WriteLine($"   ✓ Ubuntu expectation check found (sourceRef: {ubuntuCheck.SourceRef})");
                    }
                }
            }
            else
            {
                Console.WriteLine("   ✗ GetOsViaSsh step not found");
            }
            
            Console.WriteLine();
            Console.WriteLine("=== Demo completed successfully! ===");
            
            // Test round-trip XML conversion
            Console.WriteLine();
            Console.WriteLine("8. Testing XML round-trip conversion...");
            var regeneratedXml = xmlMappingService.TestToXml(test);
            var regeneratedTest = xmlMappingService.XmlToTest(regeneratedXml);
            
            var roundTripValid = regeneratedTest.Id == test.Id && 
                                regeneratedTest.Steps.Count == test.Steps.Count &&
                                regeneratedTest.Sections.Count == test.Sections.Count;
            
            if (roundTripValid)
            {
                Console.WriteLine("   ✓ XML round-trip conversion successful");
            }
            else
            {
                Console.WriteLine("   ✗ XML round-trip conversion failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
