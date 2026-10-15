using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Controls.Description;

namespace Sample;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


        };
    }

    private ScatterMagneticFieldData _scatterMagneticFieldData = null;
    public ScatterMagneticFieldData ScatterMagneticFieldData
    {
        get
        {
            if (_scatterMagneticFieldData == null)
            {
                _scatterMagneticFieldData = new ScatterMagneticFieldData(); 
            }
            return _scatterMagneticFieldData;
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
                DataChartScatterCoreDescriptionModule.Register(context);
                DataChartScatterDescriptionModule.Register(context);
                DataChartInteractivityDescriptionModule.Register(context);
                ScatterContourSeriesDescriptionModule.Register(context);
                ValueBrushScaleDescriptionModule.Register(context);
                LinearContourValueResolverDescriptionModule.Register(context);
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

