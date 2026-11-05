using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ConfigForge.Models;

public partial class ConfigNode : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial object? Value { get; set; }

    [ObservableProperty]
    public partial bool IsSection { get; set; }

    [ObservableProperty]
    public partial bool IsArray { get; set; }

    [ObservableProperty]
    public partial List<ConfigNode> Children { get; set; } = new();
}
