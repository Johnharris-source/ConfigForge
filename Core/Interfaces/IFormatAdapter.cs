
using System.Collections.Generic;
using ConfigForge.Models;

namespace ConfigForge.Core.Interfaces;

public interface IFormatAdapter
{
    string FormatName { get; }

    string FileExtension { get; }

    List<ConfigNode> Parse(string source);

    string Serialize(List<ConfigNode> nodes);
}
