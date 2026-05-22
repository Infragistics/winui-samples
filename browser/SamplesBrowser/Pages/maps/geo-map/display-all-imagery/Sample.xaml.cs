using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Portable.Description;
using Infragistics.Controls.Description;
using Infragistics.Controls.Layouts;
using Infragistics.Controls.Maps;
using System;

namespace SamplesBrowser.Pages.Maps.GeoMap.DisplayAllImagery;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{

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
                this._componentRenderer = ComponentRendererHelper.CreateRenderer();
                var context = this._componentRenderer.Context;
                PropertyEditorPanelDescriptionModule.Register(context);
                DataChartInteractivityDescriptionModule.Register(context);
                GeographicMapDescriptionModule.Register(context);
                ArcGISOnlineMapImageryDescriptionModule.Register(context);
            }
        return this._componentRenderer;
        }
    }

    //WPF: Infragistics.Controls.Layouts.PropertyEditorPropertyDescriptionChangedEventHandler
    public void EditorChangeImagerySource(object sender, PropertyEditorPropertyDescriptionChangedEventArgs args)
    {
        var map = this.map;
        var name = args.NewValue.ToString();
        if (name == "OpenStreetMaps")
        {
            map.BackgroundContent = new OpenStreetMapImagery();
        }
        else
        {
            var imagery = new ArcGISOnlineMapImagery();
            imagery.MapServerUri = name;
            map.BackgroundContent = imagery;
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
