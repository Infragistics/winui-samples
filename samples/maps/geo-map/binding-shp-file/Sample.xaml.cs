using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Controls.Maps;
using System;

namespace Sample;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


            this.MapBindingShpFileLoad();
        };
    }

    private MapShpFileReaders mapShpFileReaders = new MapShpFileReaders();


    /// <summary>
    /// A shapefile is fetched and parsed asynchronously, so the source is given the method that turns
    /// its records into data when they arrive — a method of the supporting item this one requires.
    /// </summary>
    public void MapBindingShpFileLoad()
    {
        var root = "https://static.infragistics.com/xplatform/shapes/";
        var readers = this.mapShpFileReaders;
    
        var sds = new ShapefileConverter();
        sds.ImportCompleted += readers.ReadRoutes;
        sds.ShapefileSource = new Uri(root + "WorldCableRoutes.shp");
        sds.DatabaseSource = new Uri(root + "WorldCableRoutes.dbf");
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

/// <summary>
/// What the shapefile becomes once it has loaded: records turned into routes, bound to the series
/// that draws them.
///
/// A handler for the shapefile converter's ImportCompleted, and the converter is created in code by
/// the item that requires this one — so there is nothing in a description to bind it to, and it is
/// not an initializer either.
/// </summary>
public class MapShpFileReaders
{
    /// <summary>Each record is one cable route, with the fields the database holds beside it.</summary>
    public void ReadRoutes(object sender, AsyncCompletedEventArgs e)
    {
        var sds = sender as ShapefileConverter;
        var geoRoutes = new List<object>();
        // parsing shapefile data and creating geo-locations
        foreach (var record in sds)
        {
            // using field/column names from .DBF file
            geoRoutes.Add(new
            {
                Points = record.Points,
                Name = record.Fields["Name"],
                Capacity = record.Fields["CapacityG"],
                Distance = record.Fields["DistanceKM"]
            });
        }

        var map = this.map;
        var lineSeries = map.Series[0] as GeographicPolylineSeries;
        lineSeries.ItemsSource = geoRoutes;
    }
}
