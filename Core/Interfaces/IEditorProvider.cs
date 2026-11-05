
using ConfigForge.Models;

namespace ConfigForge.Core.Interfaces;

public interface IEditorProvider
{
    bool CanEdit(ConfigNode node);

    string EditorType { get; }
}
