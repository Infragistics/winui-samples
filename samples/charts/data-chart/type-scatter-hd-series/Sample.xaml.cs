using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;


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

    private ScatterHighDensityData _scatterHighDensityData = null;
    public ScatterHighDensityData ScatterHighDensityData
    {
        get
        {
            if (_scatterHighDensityData == null)
            {
                _scatterHighDensityData = new ScatterHighDensityData(); 
            }
            return _scatterHighDensityData;
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

