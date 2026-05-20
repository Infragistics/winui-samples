using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SamplesBrowser.Services;

namespace SamplesBrowser;

public sealed partial class MainWindow : Window
{
    private readonly TocService _tocService = new();
    private readonly Dictionary<NavigationViewItem, string> _sampleRoutes = new();

    // Segoe MDL2 glyphs used for nav items (component header / sample leaf).
    // Kept as char-cast strings so the source file stays pure ASCII and the
    // glyph values are unambiguous (no escape-mangling through editor tools).
    private static readonly string ComponentGlyph = ((char)0xE9F9).ToString();
    private static readonly string SampleGlyph = ((char)0xE712).ToString();

    public MainWindow()
    {
        this.InitializeComponent();
        this.Title = "IG WinUI Samples";
        this.ExtendsContentIntoTitleBar = true;
    }

    private async void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        await _tocService.LoadAsync();
        BuildNavigation();
        ContentFrame.Navigate(typeof(Pages.HomePage));
    }

    private void BuildNavigation()
    {
        if (_tocService.Toc == null) return;

        NavView.MenuItems.Clear();
        _sampleRoutes.Clear();

        foreach (var group in _tocService.Toc.Groups)
        {
            // Group header (e.g. "Charts", "Gauges")
            NavView.MenuItems.Add(new NavigationViewItemHeader { Content = group.Name });

            foreach (var component in group.Components)
            {
                // Component item (e.g. "Category Chart") - expands to show samples
                var compItem = new NavigationViewItem
                {
                    Content = component.Name,
                    SelectsOnInvoked = false,
                    Icon = new FontIcon { Glyph = ComponentGlyph }
                };

                foreach (var sample in component.Samples)
                {
                    if (!sample.ShowLink) continue;

                    var sampleItem = new NavigationViewItem
                    {
                        Content = sample.Name,
                        Tag = sample.Route,
                        Icon = new FontIcon { Glyph = SampleGlyph }
                    };
                    compItem.MenuItems.Add(sampleItem);
                    _sampleRoutes[sampleItem] = sample.Route;
                }

                NavView.MenuItems.Add(compItem);
            }
        }
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked) return;

        if (args.InvokedItemContainer is NavigationViewItem item &&
            item.Tag is string route &&
            SampleRegistry.Samples.TryGetValue(route, out var factory))
        {
            NavView.Header = GetSampleTitle(route);
            ContentFrame.Navigate(typeof(Pages.SampleHostPage), factory);
        }
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        // Sync back-button visibility with navigation stack depth
        NavView.IsBackButtonVisible = ContentFrame.CanGoBack
            ? NavigationViewBackButtonVisible.Visible
            : NavigationViewBackButtonVisible.Collapsed;
    }

    private string GetSampleTitle(string route)
    {
        if (_tocService.Toc == null) return route;
        foreach (var group in _tocService.Toc.Groups)
            foreach (var comp in group.Components)
                foreach (var sample in comp.Samples)
                    if (sample.Route == route)
                        return comp.Name + " - " + sample.Name;
        return route;
    }
}
