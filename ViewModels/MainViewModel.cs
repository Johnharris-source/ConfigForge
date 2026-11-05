
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConfigForge.Services;
using ConfigForge.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System;

namespace ConfigForge.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly TomlDocumentParser _tomlParser = new();
    private readonly SchemaParser _schemaParser = new();

    private ConfigSchema? _schema;
    private bool _isUpdating;

    [ObservableProperty]
    public partial string CurrentFileName { get; set; } = "No configuration";

    [ObservableProperty]
    public partial string SourceCode { get; set; } =
        """
        # ConfigForge example

        [general]
        enabled = true
        name = "Example"
        ports = [80, 443, 8080]
        names = ["HTTP", "HTTPS", "SSH"]
        """;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Ready";

    [ObservableProperty]
    public partial List<ConfigNode> ConfigNodes { get; set; } = new();

    [ObservableProperty]
    public partial ConfigNode? SelectedNode { get; set; }

    [ObservableProperty]
    public partial bool IsBoolean { get; set; }

    [ObservableProperty]
    public partial bool IsNumber { get; set; }

    [ObservableProperty]
    public partial bool IsNumberInput { get; set; }

    [ObservableProperty]
    public partial bool IsSlider { get; set; }

    [ObservableProperty]
    public partial double SliderMinimum { get; set; }

    [ObservableProperty]
    public partial double SliderMaximum { get; set; } = 100;

    [ObservableProperty]
    public partial double SliderStep { get; set; } = 1;

    [ObservableProperty]
    public partial double SelectedSliderValue { get; set; }

    [ObservableProperty]
    public partial bool IsText { get; set; }

    [ObservableProperty]
    public partial bool IsArray { get; set; }

    [ObservableProperty]
    public partial bool IsArrayElement { get; set; }

    [ObservableProperty]
    public partial bool SelectedBooleanValue { get; set; }

    [ObservableProperty]
    public partial string SelectedTextValue { get; set; } = "";

    // Optional schema support

    public void LoadSchema(string json)
    {
        try
        {
            _schema = _schemaParser.Parse(json);
            UpdateSelectedNodeEditor(SelectedNode);

            StatusMessage =
                $"Schema loaded: {_schema.Properties.Count} properties";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Schema error: {ex.Message}";
        }
    }

    private ConfigPropertySchema? GetSchemaForNode(ConfigNode node)
    {
        if (_schema is null)
            return null;

        var path = FindNodePath(ConfigNodes, node);

        if (path is null)
            return null;

        return _schema.Properties.TryGetValue(path, out var property)
            ? property
            : null;
    }

    private static string? FindNodePath(
        List<ConfigNode> nodes,
        ConfigNode target,
        string parentPath = "")
    {
        foreach (var node in nodes)
        {
            var path = string.IsNullOrEmpty(parentPath)
                ? node.Name
                : $"{parentPath}.{node.Name}";

            if (ReferenceEquals(node, target))
                return path;

            var result = FindNodePath(node.Children, target, path);

            if (result is not null)
                return result;
        }

        return null;
    }

    partial void OnSelectedNodeChanged(ConfigNode? value)
    {
        UpdateSelectedNodeEditor(value);

        if (value is not null)
            StatusMessage = $"Selected: {value.Name}";
    }

    private void UpdateSelectedNodeEditor(ConfigNode? value)
    {
        _isUpdating = true;

        try
        {
            IsArrayElement = value is not null &&
                             FindParent(ConfigNodes, value) is { IsArray: true };

            IsArray = value?.IsArray == true || IsArrayElement;

            IsBoolean = value?.Value is bool;
            IsNumber = value?.Value is long or int or double or float;
            IsText = value?.Value is string;

            var schema = value is null ? null : GetSchemaForNode(value);

            IsSlider = IsNumber &&
                       string.Equals(
                           schema?.Widget,
                           "slider",
                           StringComparison.OrdinalIgnoreCase) &&
                       schema?.Min is not null &&
                       schema.Max is not null &&
                       schema.Min < schema.Max &&
                       schema.Step > 0;

            // Числовое поле показываем только тогда,
            // когда для параметра не используется Slider.
            IsNumberInput = IsNumber && !IsSlider;

            SliderMinimum = schema?.Min ?? 0;
            SliderMaximum = schema?.Max ?? 100;
            SliderStep = schema?.Step > 0 ? schema.Step : 1;

            SelectedSliderValue = value?.Value is not null
                ? Convert.ToDouble(
                    value.Value,
                    CultureInfo.InvariantCulture)
                : 0;

            SelectedBooleanValue =
                value?.Value is bool boolean && boolean;

            SelectedTextValue = value?.Value?.ToString() ?? "";
        }
        finally
        {
            _isUpdating = false;
        }
    }

    partial void OnSelectedBooleanValueChanged(bool value)
    {
        if (!_isUpdating && SelectedNode?.Value is bool)
            SelectedNode.Value = value;
    }

    partial void OnSelectedSliderValueChanged(double value)
    {
        if (_isUpdating || !IsSlider || SelectedNode is null)
            return;

        SelectedNode.Value = SelectedNode.Value switch
        {
            long => (long)Math.Round(value),
            int => (int)Math.Round(value),
            float => (float)value,
            double => value,
            _ => value
        };
    }

    partial void OnSelectedTextValueChanged(string value)
    {
        if (_isUpdating || SelectedNode is null)
            return;

        var oldValue = SelectedNode.Value;

        if (oldValue is string)
        {
            SelectedNode.Value = value;
        }
        else if (oldValue is long &&
                 long.TryParse(
                     value,
                     NumberStyles.Integer,
                     CultureInfo.InvariantCulture,
                     out var l))
        {
            SelectedNode.Value = l;
        }
        else if (oldValue is int &&
                 int.TryParse(
                     value,
                     NumberStyles.Integer,
                     CultureInfo.InvariantCulture,
                     out var i))
        {
            SelectedNode.Value = i;
        }
        else if (oldValue is double &&
                 double.TryParse(
                     value,
                     NumberStyles.Float,
                     CultureInfo.InvariantCulture,
                     out var d))
        {
            SelectedNode.Value = d;
        }
        else if (oldValue is float &&
                 float.TryParse(
                     value,
                     NumberStyles.Float,
                     CultureInfo.InvariantCulture,
                     out var f))
        {
            SelectedNode.Value = f;
        }
        else if (oldValue is not string)
        {
            StatusMessage = "Invalid number";
        }
    }

    partial void OnSourceCodeChanged(string value)
    {
        if (_isUpdating)
            return;

        if (CurrentFileName.EndsWith(
            ".toml",
            StringComparison.OrdinalIgnoreCase))
        {
            ParseToml();
        }
    }

    public void ParseToml()
    {
        try
        {
            ConfigNodes = _tomlParser.ParseToNodes(SourceCode);

            foreach (var node in ConfigNodes)
                SubscribeToNode(node);

            SelectedNode = null;

            StatusMessage = $"Parsed: {ConfigNodes.Count} entries";
        }
        catch (Exception ex)
        {
            StatusMessage = $"TOML error: {ex.Message}";
        }
    }

    private void SubscribeToNode(ConfigNode node)
    {
        node.PropertyChanged += OnNodePropertyChanged;

        foreach (var child in node.Children)
            SubscribeToNode(child);
    }

    private void OnNodePropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (sender is not ConfigNode node)
            return;

        if (e.PropertyName == nameof(ConfigNode.Children))
        {
            foreach (var child in node.Children)
                SubscribeToNode(child);

            RefreshSourceCode();
            return;
        }

        if (e.PropertyName == nameof(ConfigNode.Value))
            RefreshSourceCode();
    }

    private void RefreshSourceCode()
    {
        try
        {
            _isUpdating = true;

            SourceCode = _tomlParser.SerializeFromNodes(ConfigNodes);

            if (SelectedNode is not null)
            {
                SelectedTextValue =
                    SelectedNode.Value?.ToString() ?? "";

                SelectedBooleanValue =
                    SelectedNode.Value is bool b && b;

                if (IsNumber && IsSlider)
                {
                    SelectedSliderValue = Convert.ToDouble(
                        SelectedNode.Value,
                        CultureInfo.InvariantCulture);
                }
            }

            StatusMessage = "Configuration updated";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Serialization error: {ex.Message}";
        }
        finally
        {
            _isUpdating = false;
        }
    }

    [RelayCommand]
    private void AddArrayElement()
    {
        if (SelectedNode is not { IsArray: true } arrayNode)
            return;

        object? defaultValue = "";

        if (arrayNode.Children.Count > 0)
        {
            var first = arrayNode.Children[0];

            defaultValue = first.IsArray
                ? null
                : first.Value switch
                {
                    bool => false,
                    int => 0,
                    long => 0L,
                    double => 0d,
                    float => 0f,
                    string => "",
                    _ => ""
                };
        }

        var children = new List<ConfigNode>(arrayNode.Children);

        children.Add(new ConfigNode
        {
            Name = $"[{children.Count}]",
            IsArray = defaultValue is null,
            Value = defaultValue
        });

        arrayNode.Children = children;

        StatusMessage = "Array element added";
    }

    [RelayCommand]
    private void RemoveArrayElement()
    {
        if (SelectedNode is null || !IsArrayElement)
            return;

        var parent = FindParent(ConfigNodes, SelectedNode);

        if (parent is null)
            return;

        var children = new List<ConfigNode>(parent.Children);

        children.Remove(SelectedNode);

        for (int i = 0; i < children.Count; i++)
            children[i].Name = $"[{i}]";

        parent.Children = children;

        SelectedNode = parent;

        StatusMessage = "Array element removed";
    }

    private static ConfigNode? FindParent(
        List<ConfigNode> nodes,
        ConfigNode target)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Contains(target))
                return node;

            var result = FindParent(node.Children, target);

            if (result is not null)
                return result;
        }

        return null;
    }
}
