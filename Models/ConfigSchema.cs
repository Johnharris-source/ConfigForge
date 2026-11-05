using System.Collections.Generic;

namespace ConfigForge.Models;

public class ConfigSchema
{
    public Dictionary<string, ConfigPropertySchema> Properties { get; set; } = new();
}

public class ConfigPropertySchema
{
    public string Type { get; set; } = "string";

    public double? Min { get; set; }

    public double? Max { get; set; }

    public double Step { get; set; } = 1;

    public string Widget { get; set; } = "textbox";
}
