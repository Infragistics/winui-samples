using Microsoft.UI.Xaml;

namespace Sample;

public partial class App : Application
{
    private Window _window;

    public App()
    {
        Infragistics.Controls.Dashboards.WinUIDataChartDashboardTileFeature.Register();
        Infragistics.Controls.Dashboards.WinUIGeographicMapDashboardTileFeature.Register();
        Infragistics.Controls.Dashboards.WinUILinearGaugeDashboardTileFeature.Register();
        Infragistics.Controls.Dashboards.WinUIPieChartDashboardTileFeature.Register();
        Infragistics.Controls.Dashboards.WinUIRadialGaugeDashboardTileFeature.Register();

        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
