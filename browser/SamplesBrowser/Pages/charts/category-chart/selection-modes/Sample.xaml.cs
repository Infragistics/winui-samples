namespace SamplesBrowser.Pages.Charts.CategoryChart.SelectionModes;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using Infragistics.Controls;
using Infragistics.Controls.Charts;


public sealed partial class Sample : UserControl, INotifyPropertyChanged
{

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


        };
    }

    private TemperatureAverageData _temperatureAverageData = null;
    public TemperatureAverageData TemperatureAverageData
    {
        get
        {
            if (_temperatureAverageData == null)
            {
                _temperatureAverageData = new TemperatureAverageData(); 
            }
            return _temperatureAverageData;
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
