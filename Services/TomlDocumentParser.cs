
using System;
using System.Collections.Generic;
using ConfigForge.Core.Interfaces;
using ConfigForge.Models;
using Tomlyn;
using Tomlyn.Model;

namespace ConfigForge.Services;

public class TomlDocumentParser : IFormatAdapter
{
    public string FormatName => "TOML";
    public string FileExtension => ".toml";

    public List<ConfigNode> Parse(string source)
    {
        var table = TomlSerializer.Deserialize<TomlTable>(source)
            ?? throw new Exception("Invalid TOML document");

        return ConvertTable(table);
    }

    public string Serialize(List<ConfigNode> nodes)
    {
        return TomlSerializer.Serialize(ConvertNodes(nodes));
    }

    // Совместимость со старым MainViewModel
    public List<ConfigNode> ParseToNodes(string source)
    {
        return Parse(source);
    }

    public string SerializeFromNodes(List<ConfigNode> nodes)
    {
        return Serialize(nodes);
    }

    private static TomlTable ConvertNodes(List<ConfigNode> nodes)
    {
        var table = new TomlTable();

        foreach (var node in nodes)
            table[node.Name] = ConvertNodeValue(node);

        return table;
    }

    private static object? ConvertNodeValue(ConfigNode node)
    {
        if (node.IsSection)
            return ConvertNodes(node.Children);

        if (node.IsArray)
        {
            var array = new TomlArray();

            foreach (var child in node.Children)
            {
                var value = ConvertNodeValue(child);

                if (value is null)
                    throw new Exception("TOML arrays cannot contain null values");

                array.Add(value);
            }

            return array;
        }

        return node.Value;
    }

    private static List<ConfigNode> ConvertTable(TomlTable table)
    {
        var nodes = new List<ConfigNode>();

        foreach (var pair in table)
        {
            if (pair.Value is TomlTable nestedTable)
            {
                nodes.Add(new ConfigNode
                {
                    Name = pair.Key,
                    IsSection = true,
                    Children = ConvertTable(nestedTable)
                });
            }
            else if (pair.Value is TomlArray array)
            {
                nodes.Add(new ConfigNode
                {
                    Name = pair.Key,
                    IsArray = true,
                    Children = ConvertArray(array)
                });
            }
            else
            {
                nodes.Add(new ConfigNode
                {
                    Name = pair.Key,
                    Value = pair.Value
                });
            }
        }

        return nodes;
    }

    private static List<ConfigNode> ConvertArray(TomlArray array)
    {
        var nodes = new List<ConfigNode>();
        int index = 0;

        foreach (var value in array)
        {
            if (value is TomlArray nestedArray)
            {
                nodes.Add(new ConfigNode
                {
                    Name = $"[{index}]",
                    IsArray = true,
                    Children = ConvertArray(nestedArray)
                });
            }
            else
            {
                nodes.Add(new ConfigNode
                {
                    Name = $"[{index}]",
                    Value = value
                });
            }

            index++;
        }

        return nodes;
    }
}
