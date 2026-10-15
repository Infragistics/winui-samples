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

