using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Controls.Description;
using Infragistics.Controls.Layouts;
using Infragistics.Controls.Charts;

namespace Sample;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{
    public string[] dropDownNames1 { get; } = new string[] { "Alt", "Control", "Shift", "Windows", "Apple", "None" };
    public string[] dropDownValues1 { get; } = new string[] { "Alt", "Control", "Shift", "Windows", "Apple", "None" };
    public string[] dropDownNames2 { get; } = new string[] { "Alt", "Control", "Shift", "Windows", "Apple", "None" };
    public string[] dropDownValues2 { get; } = new string[] { "Alt", "Control", "Shift", "Windows", "Apple", "None" };
    public string[] dropDownNames3 { get; } = new string[] { "DragZoom", "DragPan", "None" };
    public string[] dropDownValues3 { get; } = new string[] { "DragZoom", "DragPan", "None" };

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


        };
    }

    private StockTesla _stockTesla = null;
    public StockTesla StockTesla
    {
        get
        {
            if (_stockTesla == null)
            {
                _stockTesla = new StockTesla(); 
            }
            return _stockTesla;
        }
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
                DataChartCoreDescriptionModule.Register(context);
                DataChartCategoryDescriptionModule.Register(context);
                DataChartCategoryCoreDescriptionModule.Register(context);
                DataChartFinancialCoreDescriptionModule.Register(context);
                DataChartFinancialDescriptionModule.Register(context);
                DataChartInteractivityDescriptionModule.Register(context);
            }
        return this._componentRenderer;
        }
    }

    public void EditorButtonChartPanUp(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var chart = this.chart;
        // The window's position is read-only here, so the window rectangle it is a shortcut to is
        // what moves. Taken as it stands rather than named, since the type it is differs by platform.
        var rect = chart.ActualWindowRect;
        rect.Y -= 0.05;
        chart.WindowRect = rect;
    }
    
    
    public void EditorButtonChartPanDown(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var chart = this.chart;
        // The window's position is read-only here, so the window rectangle it is a shortcut to is
        // what moves. Taken as it stands rather than named, since the type it is differs by platform.
        var rect = chart.ActualWindowRect;
        rect.Y += 0.05;
        chart.WindowRect = rect;
    }
    
    
    public void EditorButtonChartPanLeft(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var chart = this.chart;
        // The window's position is read-only here, so the window rectangle it is a shortcut to is
        // what moves. Taken as it stands rather than named, since the type it is differs by platform.
        var rect = chart.ActualWindowRect;
        rect.X -= 0.05;
        chart.WindowRect = rect;
    }
    
    
    public void EditorButtonChartPanRight(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var chart = this.chart;
        // The window's position is read-only here, so the window rectangle it is a shortcut to is
        // what moves. Taken as it stands rather than named, since the type it is differs by platform.
        var rect = chart.ActualWindowRect;
        rect.X += 0.05;
        chart.WindowRect = rect;
    }
    
    
    public void EditorButtonChartZoomIn(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var chart = this.chart;
        // The window's position and scale are read-only here, unlike on the web platforms: the chart
        // has a method that moves both together, by the same fraction the web handler applies.
        chart.ZoomIn(0.05);
    }
    
    
    public void EditorButtonChartZoomOut(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var chart = this.chart;
        // The window's position and scale are read-only here, unlike on the web platforms: the chart
        // has a method that moves both together, by the same fraction the web handler applies.
        chart.ZoomOut(0.05);
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

