using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Controls.Description;

namespace Sample;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{
    public string[] dropDownNames1 { get; } = new string[] { "Road", "Satellite", "DarkGrey", "LabelsRoadOverlay", "LabelsDarkGreyOverlay", "HybridRoadOverlay", "HybridDarkGreyOverlay", "TerraOverlay", "WeatherRadarOverlay", "WeatherInfraredOverlay", "TrafficAbsoluteOverlay", "TrafficRelativeOverlay", "TrafficRelativeDarkOverlay", "TrafficDelayOverlay", "TrafficReducedOverlay" };
    public string[] dropDownValues1 { get; } = new string[] { "Road", "Satellite", "DarkGrey", "LabelsRoadOverlay", "LabelsDarkGreyOverlay", "HybridRoadOverlay", "HybridDarkGreyOverlay", "TerraOverlay", "WeatherRadarOverlay", "WeatherInfraredOverlay", "TrafficAbsoluteOverlay", "TrafficRelativeOverlay", "TrafficRelativeDarkOverlay", "TrafficDelayOverlay", "TrafficReducedOverlay" };

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


        };
    }


    private ComponentRenderer _componentRenderer = null;
    public ComponentRenderer Renderer
    {
        get
        {
            if (this._componentRenderer == null)
            {
                this._componentRenderer = new ComponentRenderer();
                var context = this._componentRenderer.Context;
                PropertyEditorPanelDescriptionModule.Register(context);
                DataChartInteractivityDescriptionModule.Register(context);
                GeographicMapDescriptionModule.Register(context);
                AzureMapsImageryDescriptionModule.Register(context);
            }
        return this._componentRenderer;
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChangedEventHandler handler = PropertyChanged;
        if (handler != null)
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

