using System;
using Infragistics.Controls.Dashboards;
using Infragistics.Controls.Grids;
using Microsoft.UI.Xaml;

namespace SamplesBrowser;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Infragistics.Core.WinUIPlatformRegistration.Register();
        XamDataGrid.IsCanvasModeDisabled = true;
        WinUIDataChartDashboardTileFeature.Register();
        WinUIPieChartDashboardTileFeature.Register();
        WinUIRadialGaugeDashboardTileFeature.Register();
        WinUILinearGaugeDashboardTileFeature.Register();
        WinUIGeographicMapDashboardTileFeature.Register();

        _window = new MainWindow();
        _window.Activate();
    }
}
