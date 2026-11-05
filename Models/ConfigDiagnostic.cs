
namespace ConfigForge.Models;

public class ConfigDiagnostic
{
    public string Message { get; set; } = "";

    public string Severity { get; set; } = "Error";

    public string? NodePath { get; set; }
}
