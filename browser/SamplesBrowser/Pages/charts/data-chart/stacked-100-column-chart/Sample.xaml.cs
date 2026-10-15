namespace SamplesBrowser.Pages.Charts.DataChart.Stacked100ColumnChart;

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

    private OnlineTrafficByDevice _onlineTrafficByDevice = null;
    public OnlineTrafficByDevice OnlineTrafficByDevice
    {
        get
        {
            if (_onlineTrafficByDevice == null)
            {
                _onlineTrafficByDevice = new OnlineTrafficByDevice(); 
            }
            return _onlineTrafficByDevice;
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
