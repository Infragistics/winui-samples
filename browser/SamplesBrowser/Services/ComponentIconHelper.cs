namespace SamplesBrowser.Services;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

/// <summary>Shared icon/brush helpers used by both MainWindow and ComponentPage.</summary>
internal static class ComponentIconHelper
{
    private const string LineBase = "ms-appx:///Assets/icons/components/line/";

    private const string SparklinePathData =
        "M 14.039062 1.4921875 A 1 1 0 0 0 13.039062 2.4921875 A 1 1 0 0 0 13.087891 2.7988281 L 10.240234 5.9785156 A 1 1 0 0 0 10.054688 5.9628906 A 1 1 0 0 0 9.5839844 6.0800781 L 7.0390625 4.3886719 A 1 1 0 0 0 6.0410156 3.4648438 A 1 1 0 0 0 5.0410156 4.4648438 A 1 1 0 0 0 5.0546875 4.6269531 L 2.2734375 7.5859375 A 1 1 0 0 1 2.2714844 7.5859375 A 1 1 0 0 0 1.9960938 7.546875 A 1 1 0 0 0 0.99609375 8.546875 A 1 1 0 0 0 1.9960938 9.546875 A 1 1 0 0 0 2.9960938 8.546875 A 1 1 0 0 0 2.9667969 8.3066406 L 5.6972656 5.4042969 A 1 1 0 0 0 6.0410156 5.4648438 A 1 1 0 0 0 6.5976562 5.296875 L 9.0546875 6.9296875 A 1 1 0 0 0 9.0546875 6.9628906 A 1 1 0 0 0 10.054688 7.9628906 A 1 1 0 0 0 11.054688 6.9628906 A 1 1 0 0 0 11 6.6328125 L 13.830078 3.4707031 A 1 1 0 0 0 14.039062 3.4921875 A 1 1 0 0 0 15.039062 2.4921875 A 1 1 0 0 0 14.039062 1.4921875 z M 5.9453125 9.9765625 A 1 1 0 0 0 4.9492188 10.876953 L 2.5273438 12.128906 A 1 1 0 0 0 1.9804688 11.966797 A 1 1 0 0 0 0.98046875 12.966797 A 1 1 0 0 0 1.9804688 13.966797 A 1 1 0 0 0 2.9785156 13.019531 L 5.3632812 11.789062 A 1 1 0 0 0 5.9453125 11.976562 A 1 1 0 0 0 6.3945312 11.869141 L 9.0371094 13.892578 A 1 1 0 0 0 9.0332031 13.988281 A 1 1 0 0 0 10.033203 14.988281 A 1 1 0 0 0 11.029297 14.082031 A 1 1 0 0 0 11.029297 14.080078 L 13.390625 12.783203 A 1 1 0 0 0 13.976562 12.974609 A 1 1 0 0 0 14.976562 11.974609 A 1 1 0 0 0 13.976562 10.974609 A 1 1 0 0 0 12.982422 11.865234 A 1 1 0 0 0 12.982422 11.867188 L 10.609375 13.169922 A 1 1 0 0 0 10.033203 12.988281 A 1 1 0 0 0 9.6191406 13.076172 L 6.9433594 11.03125 A 1 1 0 0 0 6.9453125 10.976562 A 1 1 0 0 0 5.9453125 9.9765625 z ";

    private const string BulletGraphPathData =
        "M 6 0 A 0.5 0.5 0 0 0 5.5 0.5 L 5.5 7.5 A 0.5 0.5 0 0 0 6 8 A 0.5 0.5 0 0 0 6.5 7.5 L 6.5 0.5 A 0.5 0.5 0 0 0 6 0 z M 2.4980469 1.8339844 C 1.6755426 1.8339844 0.99804687 2.5114801 0.99804688 3.3339844 L 0.99804688 4.6660156 C 0.99804688 5.4885148 1.6755365 6.1660156 2.4980469 6.1660156 L 4 6.1660156 A 0.5 0.5 0 0 0 4.5 5.6660156 A 0.5 0.5 0 0 0 4 5.1660156 L 2.4980469 5.1660156 C 2.2159772 5.1660156 1.9980469 4.9480764 1.9980469 4.6660156 L 1.9980469 3.3339844 C 1.9980469 3.0519087 2.2159712 2.8339844 2.4980469 2.8339844 L 4 2.8339844 A 0.5 0.5 0 0 0 4.5 2.3339844 A 0.5 0.5 0 0 0 4 1.8339844 L 2.4980469 1.8339844 z M 8 1.8339844 A 0.5 0.5 0 0 0 7.5 2.3339844 A 0.5 0.5 0 0 0 8 2.8339844 L 10 2.8339844 L 10 5.1660156 L 8 5.1660156 A 0.5 0.5 0 0 0 7.5 5.6660156 A 0.5 0.5 0 0 0 8 6.1660156 L 10.5 6.1660156 L 13.503906 6.1660156 C 14.326422 6.1660156 15.003906 5.4885088 15.003906 4.6660156 L 15.003906 3.3339844 C 15.003906 2.5114912 14.326422 1.8339844 13.503906 1.8339844 L 10.5 1.8339844 L 8 1.8339844 z M 11 2.8339844 L 13.503906 2.8339844 C 13.785991 2.8339844 14.003906 3.0519176 14.003906 3.3339844 L 14.003906 4.6660156 C 14.003906 4.9480824 13.785991 5.1660156 13.503906 5.1660156 L 11 5.1660156 L 11 2.8339844 z M 10 8 A 0.5 0.5 0 0 0 9.5 8.5 L 9.5 15.5 A 0.5 0.5 0 0 0 10 16 A 0.5 0.5 0 0 0 10.5 15.5 L 10.5 8.5 A 0.5 0.5 0 0 0 10 8 z M 2.4980469 9.8300781 C 1.6755486 9.8300781 0.99804687 10.507569 0.99804688 11.330078 L 0.99804688 12.664062 C 0.99804688 13.486572 1.6755486 14.164062 2.4980469 14.164062 L 5.5019531 14.164062 L 8 14.164062 A 0.5 0.5 0 0 0 8.5 13.664062 A 0.5 0.5 0 0 0 8 13.164062 L 6.0019531 13.164062 L 6.0019531 10.830078 L 8 10.830078 A 0.5 0.5 0 0 0 8.5 10.330078 A 0.5 0.5 0 0 0 8 9.8300781 L 5.5019531 9.8300781 L 2.4980469 9.8300781 z M 12 9.8300781 A 0.5 0.5 0 0 0 11.5 10.330078 A 0.5 0.5 0 0 0 12 10.830078 L 13.501953 10.830078 C 13.78405 10.830078 14.001953 11.047982 14.001953 11.330078 L 14.001953 12.664062 C 14.001953 12.946158 13.78405 13.164062 13.501953 13.164062 L 12 13.164062 A 0.5 0.5 0 0 0 11.5 13.664062 A 0.5 0.5 0 0 0 12 14.164062 L 13.501953 14.164062 C 14.324457 14.164062 15.001953 13.486565 15.001953 12.664062 L 15.001953 11.330078 C 15.001953 10.507575 14.324457 9.8300781 13.501953 9.8300781 L 12 9.8300781 z M 2.4980469 10.830078 L 5.0019531 10.830078 L 5.0019531 13.164062 L 2.4980469 13.164062 C 2.2159652 13.164062 1.9980469 12.946153 1.9980469 12.664062 L 1.9980469 11.330078 C 1.9980469 11.047988 2.2159652 10.830078 2.4980469 10.830078 z ";

    private const string RadialGaugePathData =
        "M 8 0.5 C 3.8637894 0.5 0.5 3.8637894 0.5 8 C 0.5 12.13622 3.8637903 15.5 8 15.5 C 12.136219 15.5 15.5 12.136219 15.5 8 C 15.5 3.8637903 12.13622 0.5 8 0.5 z M 8 1.5 C 11.59578 1.5 14.5 4.4042297 14.5 8 C 14.5 11.595781 11.595781 14.5 8 14.5 C 4.4042297 14.5 1.5 11.59578 1.5 8 C 1.5 4.4042306 4.4042306 1.5 8 1.5 z M 8.4472656 2.3730469 C 5.0992842 2.3730469 2.3730469 5.0973311 2.3730469 8.4453125 A 0.5 0.5 0 0 0 2.8730469 8.9453125 A 0.5 0.5 0 0 0 3.3730469 8.4453125 C 3.3730469 5.6377739 5.639727 3.3730469 8.4472656 3.3730469 A 0.5 0.5 0 0 0 8.9472656 2.8730469 A 0.5 0.5 0 0 0 8.4472656 2.3730469 z M 11.466797 4.5019531 A 0.5 0.5 0 0 0 11.169922 4.625 L 8.6640625 6.8222656 C 8.4449598 6.6979172 8.2675081 6.5 8 6.5 C 7.1774966 6.5 6.5 7.1774966 6.5 8 C 6.5 8.8225034 7.1774966 9.5 8 9.5 C 8.8225034 9.5 9.5 8.8225034 9.5 8 C 9.5 7.8330284 9.3968426 7.7060177 9.3457031 7.5546875 L 11.830078 5.375 A 0.5 0.5 0 0 0 11.875 4.6699219 A 0.5 0.5 0 0 0 11.466797 4.5019531 z M 8 7.5 C 8.1627527 7.5 8.3006052 7.574803 8.3925781 7.6914062 C 8.4600108 7.7768922 8.5 7.880652 8.5 8 C 8.5 8.2820966 8.2820966 8.5 8 8.5 C 7.7179034 8.5 7.5 8.2820966 7.5 8 C 7.5 7.7179034 7.7179034 7.5 8 7.5 z ";

    public static IconElement NavIconForComponent(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.Contains("sparkline"))
            return new PathIcon { Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{SparklinePathData}</Geometry>") };

        if (n.Contains("bullet"))
            return new PathIcon { Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{BulletGraphPathData}</Geometry>") };

        if (n.Contains("radial gauge") || n.Contains("radialgauge"))
            return new PathIcon { Data = (Geometry)Microsoft.UI.Xaml.Markup.XamlReader.Load(
                $"<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>{RadialGaugePathData}</Geometry>") };

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
        var n when n.Contains("bullet")         => $"{LineBase}icon-bulletgraph-line.svg",
        var n when n.Contains("radial gauge")   => $"{LineBase}icon-radialgauge-line.svg",
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
        var n when n.Contains("bullet")         => "\uEB4C", // ProgressBar
        var n when n.Contains("radial gauge")   => "\uEA18", // Dial / gauge
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
        var n when n.Contains("bullet")         => "icon-bulletgraph-inverted.svg",
        var n when n.Contains("radial gauge")   => "icon-radialgauge-inverted.svg",
        var n when n.Contains("toolbar")        => "icon-toolbar-inverted.svg",
        var n when n.Contains("tree")           => "icon-treemap-inverted.svg",
        var n when n.Contains("color")          => "icon-color-inverted.svg",
        _                                       => "icon-component-inverted.svg"
    };
}
