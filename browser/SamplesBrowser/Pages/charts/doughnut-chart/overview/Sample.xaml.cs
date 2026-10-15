namespace SamplesBrowser.Pages.Charts.DoughnutChart.Overview;

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

    private CompanyMarketShares _companyMarketShares = null;
    public CompanyMarketShares CompanyMarketShares
    {
        get
        {
            if (_companyMarketShares == null)
            {
                _companyMarketShares = new CompanyMarketShares(); 
            }
            return _companyMarketShares;
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
