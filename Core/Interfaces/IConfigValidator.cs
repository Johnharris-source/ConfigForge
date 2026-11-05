
using System.Collections.Generic;
using ConfigForge.Models;

namespace ConfigForge.Core.Interfaces;

public interface IConfigValidator
{
    List<ConfigDiagnostic> Validate(List<ConfigNode> nodes);
}
