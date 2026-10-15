using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Portable.Description;
using Infragistics.Controls.Description;
using Infragistics.Controls;
using Infragistics.Controls.Charts;
using Infragistics.Controls.Layouts;

namespace Sample;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{
    private FormatSpecifierCollection _numberFormatSpecifier1 = null;
    public FormatSpecifierCollection numberFormatSpecifier1
    {
        get
        {
            if (_numberFormatSpecifier1 == null)
            {
                var numberFormatSpecifier1 = new FormatSpecifierCollection();
                var numberFormatSpecifier2 = new NumberFormatSpecifier();
                numberFormatSpecifier2.Style = "currency";
                numberFormatSpecifier2.Currency = "USD";
                numberFormatSpecifier2.CurrencyDisplay = "symbol";
                numberFormatSpecifier2.MaximumFractionDigits = 2;
                numberFormatSpecifier2.MinimumFractionDigits = 2;
                
                numberFormatSpecifier1.Add(numberFormatSpecifier2);
                _numberFormatSpecifier1 = numberFormatSpecifier1;
            }
            return _numberFormatSpecifier1;
        }
    }
    private FormatSpecifierCollection _numberFormatSpecifier3 = null;
    public FormatSpecifierCollection numberFormatSpecifier3
    {
        get
        {
            if (_numberFormatSpecifier3 == null)
            {
                var numberFormatSpecifier3 = new FormatSpecifierCollection();
                var numberFormatSpecifier4 = new NumberFormatSpecifier();
                numberFormatSpecifier4.Style = "currency";
                numberFormatSpecifier4.Currency = "USD";
                numberFormatSpecifier4.CurrencyDisplay = "symbol";
                numberFormatSpecifier4.MaximumFractionDigits = 2;
                numberFormatSpecifier4.MinimumFractionDigits = 2;
                
                numberFormatSpecifier3.Add(numberFormatSpecifier4);
                _numberFormatSpecifier3 = numberFormatSpecifier3;
            }
            return _numberFormatSpecifier3;
        }
    }
    private FormatSpecifierCollection _numberFormatSpecifier5 = null;
    public FormatSpecifierCollection numberFormatSpecifier5
    {
        get
        {
            if (_numberFormatSpecifier5 == null)
            {
                var numberFormatSpecifier5 = new FormatSpecifierCollection();
                var numberFormatSpecifier6 = new NumberFormatSpecifier();
                numberFormatSpecifier6.Style = "currency";
                numberFormatSpecifier6.Currency = "USD";
                numberFormatSpecifier6.CurrencyDisplay = "symbol";
                numberFormatSpecifier6.MinimumFractionDigits = 0;
                
                numberFormatSpecifier5.Add(numberFormatSpecifier6);
                _numberFormatSpecifier5 = numberFormatSpecifier5;
            }
            return _numberFormatSpecifier5;
        }
    }

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


        };
    }

    private HighestGrossingMovies _highestGrossingMovies = null;
    public HighestGrossingMovies HighestGrossingMovies
    {
        get
        {
            if (_highestGrossingMovies == null)
            {
                _highestGrossingMovies = new HighestGrossingMovies(); 
            }
            return _highestGrossingMovies;
        }
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
                DataLegendDescriptionModule.Register(context);
                CategoryChartDescriptionModule.Register(context);
                NumberFormatSpecifierDescriptionModule.Register(context);
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

