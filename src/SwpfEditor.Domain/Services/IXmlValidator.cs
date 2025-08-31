using SwpfEditor.Domain.Models;

namespace SwpfEditor.Domain.Services;

public interface IXmlValidator
{
    /// <summary>
    /// Validates XML against XSD schema
    /// </summary>
    /// <param name="xmlContent">XML content to validate</param>
    /// <param name="schemaPath">Path to XSD schema file</param>
    /// <returns>List of validation errors</returns>
    List<ValidationError> ValidateAgainstSchema(string xmlContent, string schemaPath);
    
    /// <summary>
    /// Validates business rules not covered by XSD
    /// </summary>
    /// <param name="test">Test model to validate</param>
    /// <param name="testConfiguration">Test configuration for additional validation</param>
    /// <returns>List of validation errors</returns>
    List<ValidationError> ValidateBusinessRules(Test test, TestConfiguration? testConfiguration = null);
    
    /// <summary>
    /// Performs comprehensive validation (XSD + business rules + placeholders + connections)
    /// </summary>
    /// <param name="xmlContent">XML content</param>
    /// <param name="test">Parsed test model</param>
    /// <param name="testConfiguration">Test configuration</param>
    /// <returns>List of all validation errors</returns>
    List<ValidationError> ValidateComprehensive(string xmlContent, Test test, TestConfiguration? testConfiguration = null);
}