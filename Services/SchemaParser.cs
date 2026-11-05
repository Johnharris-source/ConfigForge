using System.Collections.Generic;
using System.Text.Json;
using ConfigForge.Models;

namespace ConfigForge.Services;

public class SchemaParser
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public ConfigSchema Parse(string json)
    {
        var properties =
            JsonSerializer.Deserialize<Dictionary<string, ConfigPropertySchema>>(
                json, _options);

        return new ConfigSchema
        {
            Properties = properties
                ?? throw new System.Exception("Schema is empty or invalid")
        };
    }
}
