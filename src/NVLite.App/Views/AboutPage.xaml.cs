using System.Reflection;
using Microsoft.UI.Xaml.Controls;

namespace NVLite.App.Views;

public sealed partial class AboutPage : Page
{
    public string AppVersion { get; } =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "unknown";

    public string RuntimeInfo { get; } =
        $".NET {Environment.Version} | {Environment.OSVersion}";

    public AboutPage()
    {
        InitializeComponent();
    }
}
