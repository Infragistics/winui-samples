using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Controls.Maps;
using System;
using Infragistics.Controls;
using Infragistics.Controls.Charts;

namespace Sample;

public sealed partial class Sample : UserControl, INotifyPropertyChanged
{

    public Sample()
    {

        this.InitializeComponent();

        DataContext = this;

        this.Loaded += (s, e) => {


            this.MapMultipleShapesLoad();
        };
    }

    private WorldCities _worldCities = null;
    public WorldCities WorldCities
    {
        get
        {
            if (_worldCities == null)
            {
                _worldCities = new WorldCities(); 
            }
            return _worldCities;
        }
    }
    
    private MapMultipleShapesReaders mapMultipleShapesReaders = new MapMultipleShapesReaders();


    /// <summary>
    /// Three shapefiles, each read into the series that draws it. A shapefile is fetched and parsed
    /// asynchronously, so each source is given the method that turns its records into data when it
    /// arrives — those methods belong to the supporting item this one requires.
    /// </summary>
    public void MapMultipleShapesLoad()
    {
        var root = "https://static.infragistics.com/xplatform/shapes/";
        var readers = this.mapMultipleShapesReaders;
    
        var sdsPolygons = new ShapefileConverter();
        sdsPolygons.ImportCompleted += readers.ReadPolygons;
        sdsPolygons.ShapefileSource = new Uri(root + "WorldCountries.shp");
        sdsPolygons.DatabaseSource = new Uri(root + "WorldCountries.dbf");
    
        var sdsPolylines = new ShapefileConverter();
        sdsPolylines.ImportCompleted += readers.ReadPolylines;
        sdsPolylines.ShapefileSource = new Uri(root + "WorldCableRoutes.shp");
        sdsPolylines.DatabaseSource = new Uri(root + "WorldCableRoutes.dbf");
    
        var sdsLocations = new ShapefileConverter();
        sdsLocations.ImportCompleted += readers.ReadPoints;
        sdsLocations.ShapefileSource = new Uri(root + "WorldCities.shp");
        sdsLocations.DatabaseSource = new Uri(root + "WorldCities.dbf");
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
/// What each of the three shapefiles becomes once it has loaded: records turned into data, bound to
/// the series that draws them.
///
/// These are handlers for a shapefile converter's ImportCompleted, and the converter is created in
/// code by the item that requires this one — so there is nothing in a description to bind them to,
/// and they are not initializers either. A supporting item is what they are: a type asked for by
/// name, whose methods the loader wires up.
/// </summary>
public class MapMultipleShapesReaders
{
    /// <summary>Country shapes, with the fields the tooltip shows taken from the .DBF file.</summary>
    public void ReadPolygons(object sender, AsyncCompletedEventArgs e)
    {
        var sds = sender as ShapefileConverter;
        var geoPolygons = new List<object>();
        // parsing shapefile data and creating geo-polygons
        foreach (var record in sds)
        {
            // using field/column names from .DBF file
            geoPolygons.Add(new
            {
                Points = record.Points,
                Name = record.Fields["NAME"],
                Population = record.Fields["POP_2005"]
            });
        }

        var map = this.geoMap;
        var polygonSeries = map.Series[0] as GeographicShapeSeries;
        polygonSeries.ItemsSource = geoPolygons;
    }

    /// <summary>Connections between places, each record one path.</summary>
    public void ReadPolylines(object sender, AsyncCompletedEventArgs e)
    {
        var sds = sender as ShapefileConverter;
        var geoPolylines = new List<object>();
        // parsing shapefile data and creating geo-polylines
        foreach (var record in sds)
        {
            // using field/column names from .DBF file
            geoPolylines.Add(new
            {
                Points = record.Points,
                Name = record.Fields["Name"],
                Capacity = record.Fields["CapacityG"],
                Distance = record.Fields["DistanceKM"],
                IsOverLand = (double)record.Fields["OverLand"] == 0,
                IsActive = (double)record.Fields["NotLive"] != 0,
                Service = record.Fields["InService"]
            });
        }

        var map = this.geoMap;
        var lineSeries = map.Series[1] as GeographicPolylineSeries;
        lineSeries.ItemsSource = geoPolylines;
    }

    /// <summary>Cities with a known population, one point per record.</summary>
    public void ReadPoints(object sender, AsyncCompletedEventArgs e)
    {
        var sds = sender as ShapefileConverter;
        var geoLocations = new List<object>();
        // parsing shapefile data and creating geo-locations
        foreach (var record in sds)
        {
            var pop = (double)record.Fields["POPULATION"];
            if (pop > 0)
            {
                // each of these records holds a single point
                // using field/column names from .DBF file
                geoLocations.Add(new
                {
                    Latitude = record.Points[0][0].Y,
                    Longitude = record.Points[0][0].X,
                    City = record.Fields["NAME"],
                    Population = pop
                });
            }
        }

        var map = this.geoMap;
        var symbolSeries = map.Series[2] as GeographicSymbolSeries;
        symbolSeries.ItemsSource = geoLocations;
    }
}
