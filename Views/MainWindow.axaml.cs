
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConfigForge.Views;

public partial class MainWindow : Window
{
    private IStorageFile? _currentFile;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ImportButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        var file = await OpenConfigurationFile();

        if (file is null)
            return;

        var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);

        var content = await reader.ReadToEndAsync();

        if (DataContext is ConfigForge.ViewModels.MainViewModel vm)
        {
            _currentFile = file;
            vm.CurrentFileName = file.Name;
            vm.SourceCode = content;

            if (!file.Name.EndsWith(".toml", StringComparison.OrdinalIgnoreCase))
                vm.StatusMessage = $"Imported: {file.Name}";
        }
    }

    private async void ImportSchemaButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Import JSON schema",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON schema")
                    {
                        Patterns = new[] { "*.json" }
                    }
                }
            });

        if (files.Count == 0)
            return;

        var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream);

        var json = await reader.ReadToEndAsync();

        if (DataContext is ConfigForge.ViewModels.MainViewModel vm)
        {
            vm.LoadSchema(json);
        }
    }

    public async Task<IStorageFile?> OpenConfigurationFile()
    {
        if (DataContext is ConfigForge.ViewModels.MainViewModel vm)
            vm.StatusMessage = "Opening file picker...";

        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Import configuration",
                AllowMultiple = false
            });

        return files.Count > 0 ? files[0] : null;
    }

    private async void SaveButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_currentFile is null)
            return;

        if (DataContext is not ConfigForge.ViewModels.MainViewModel vm)
            return;

        await using var stream = await _currentFile.OpenWriteAsync();
        stream.SetLength(0);

        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(vm.SourceCode);
        await writer.FlushAsync();

        vm.StatusMessage = $"Saved: {_currentFile.Name}";
    }
}
