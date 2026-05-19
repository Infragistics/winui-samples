namespace SamplesBrowser.Services;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

/// <summary>Shared icon/brush helpers used by both MainWindow and ComponentPage.</summary>
internal static class ComponentIconHelper
{
    private const string LineBase = "ms-appx:///Assets/icons/components/line/";

    private const string SparklinePathData =
        "M 14.039062 1.4921875 A 1 1 0 0 0 13.039062 2.4921875 A 1 1 0 0 0 13.087891 2.7988281 L 10.240234 5.9785156 A 1 1 0 0 0 10.054688 5.9628906 A 1 1 0 0 0 9.5839844 6.0800781 L 7.0390625 4.3886719 A 1 1 0 0 0 6.0410156 3.4648438 A 1 1 0 0 0 5.0410156 4.4648438 A 1 1 0 0 0 5.0546875 4.6269531 L 2.2734375 7.5859375 A 1 1 0 0 1 2.2714844 7.5859375 A 1 1 0 0 0 1.9960938 7.546875 A 1 1 0 0 0 0.99609375 8.546875 A 1 1 0 0 0 1.9960938 9.546875 A 1 1 0 0 0 2.9960938 8.546875 A 1 1 0 0 0 2.9667969 8.3066406 L 5.6972656 5.4042969 A 1 1 0 0 0 6.0410156 5.4648438 A 1 1 0 0 0 6.5976562 5.296875 L 9.0546875 6.9296875 A 1 1 0 0 0 9.0546875 6.9628906 A 1 1 0 0 0 10.054688 7.9628906 A 1 1 0 0 0 11.054688 6.9628906 A 1 1 0 0 0 11 6.6328125 L 13.830078 3.4707031 A 1 1 0 0 0 14.039062 3.4921875 A 1 1 0 0 0 15.039062 2.4921875 A 1 1 0 0 0 14.039062 1.4921875 z M 5.9453125 9.9765625 A 1 1 0 0 0 4.9492188 10.876953 L 2.5273438 12.128906 A 1 1 0 0 0 1.9804688 11.966797 A 1 1 0 0 0 0.98046875 12.966797 A 1 1 0 0 0 1.9804688 13.966797 A 1 1 0 0 0 2.9785156 13.019531 L 5.3632812 11.789062 A 1 1 0 0 0 5.9453125 11.976562 A 1 1 0 0 0 6.3945312 11.869141 L 9.0371094 13.892578 A 1 1 0 0 0 9.0332031 13.988281 A 1 1 0 0 0 10.033203 14.988281 A 1 1 0 0 0 11.029297 14.082031 A 1 1 0 0 0 11.029297 14.080078 L 13.390625 12.783203 A 1 1 0 0 0 13.976562 12.974609 A 1 1 0 0 0 14.976562 11.974609 A 1 1 0 0 0 13.976562 10.974609 A 1 1 0 0 0 12.982422 11.865234 A 1 1 0 0 0 12.982422 11.867188 L 10.609375 13.169922 A 1 1 0 0 0 10.033203 12.988281 A 1 1 0 0 0 9.6191406 13.076172 L 6.9433594 11.03125 A 1 1 0 0 0 6.9453125 10.976562 A 1 1 0 0 0 5.9453125 9.9765625 z ";

    public static IconElement NavIconForComponent(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("sparkline"))
            return new PathIcon { Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{SparklinePathData}</Geometry>") };

        return new FontIcon { Glyph = NavGlyphForComponent(name), FontSize = 16 };
    }

    public static Uri LineIconUriForComponent(string name) => new(name.ToLowerInvariant() switch
    {
        var n when n.Contains("category chart") => $"{LineBase}icon-categorychart-line.svg",
        var n when n.Contains("dashboard")      => $"{LineBase}icon-dashboard-line.svg",
        var n when n.Contains("data pie")       => $"{LineBase}icon-pie-line.svg",
        var n when n.Contains("data chart")     => $"{LineBase}icon-datachart-line.svg",
        var n when n.Contains("doughnut")       => $"{LineBase}icon-doughnut-line.svg",
        var n when n.Contains("pie")            => $"{LineBase}icon-pie-line.svg",
        var n when n.Contains("sparkline")      => $"{LineBase}icon-sparkline-line.svg",
        var n when n.Contains("toolbar")        => $"{LineBase}icon-toolbar-line.svg",
        var n when n.Contains("tree")           => $"{LineBase}icon-treemap-line.svg",
        var n when n.Contains("color")          => $"{LineBase}icon-color-line.svg",
        _                                       => $"{LineBase}icon-datachart-line.svg"
    });

    public static string NavGlyphForComponent(string name) => name.ToLowerInvariant() switch
    {
        var n when n.Contains("category chart") => "\uE9D2", // Chart / bar chart
        var n when n.Contains("dashboard")      => "\uECA5", // LayoutColumn / grid tiles
        var n when n.Contains("data pie")       => "\uEB05", // PieSingle
        var n when n.Contains("data chart")     => "\uE9D2", // AreaChart
        var n when n.Contains("doughnut")       => "\uEB05", // PieSingle (closest ring)
        var n when n.Contains("pie")            => "\uEB05", // PieSingle
        var n when n.Contains("sparkline")      => "\uEB66", // Sparkline / trend line
        var n when n.Contains("toolbar")        => "\uEC7A", // ToolBox
        var n when n.Contains("tree")           => "\uF246", // Treemap
        var n when n.Contains("color")          => "\uE790", // ColorSwatch
        _                                       => "\uF0E2"
    };

    public static string InvertedIconFileForComponent(string name) => name.ToLowerInvariant() switch
    {
        var n when n.Contains("category chart") => "icon-categorychart-inverted.svg",
        var n when n.Contains("dashboard")      => "icon-dashboard-inverted.svg",
        var n when n.Contains("data pie")       => "icon-pie-inverted.svg",
        var n when n.Contains("data chart")     => "icon-datachart-inverted.svg",
        var n when n.Contains("doughnut")       => "icon-doughnut-inverted.svg",
        var n when n.Contains("pie")            => "icon-pie-inverted.svg",
        var n when n.Contains("sparkline")      => "icon-sparkline-inverted.svg",
        var n when n.Contains("toolbar")        => "icon-toolbar-inverted.svg",
        var n when n.Contains("tree")           => "icon-treemap-inverted.svg",
        var n when n.Contains("color")          => "icon-color-inverted.svg",
        _                                       => "icon-component-inverted.svg"
    };
}
