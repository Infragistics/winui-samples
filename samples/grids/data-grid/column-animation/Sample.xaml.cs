using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

using Infragistics.Portable.Description;
using Infragistics.Controls.Description;
using Infragistics.Controls.Layouts;
using Infragistics.Controls.Grids;
using System;
using System.Collections.Generic;

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

    private EmployeesSalesData _employeesSalesData = null;
    public EmployeesSalesData EmployeesSalesData
    {
        get
        {
            if (_employeesSalesData == null)
            {
                _employeesSalesData = new EmployeesSalesData();
            }
            return _employeesSalesData;
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
                DataGridDescriptionModule.Register(context);
            }
        return this._componentRenderer;
        }
    }

    //WPF: Infragistics.Controls.Layouts.PropertyEditorPropertyDescriptionButtonClickEventHandler
    public void DataGridHideFirstVisibleColumn(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var grid = this.grid;
        for (var i = 0; i < grid.ActualColumns.Count; i++)
        {
            var col = grid.ActualColumns[i];
            if (!col.IsHidden)
            {
                col.IsHidden = true;
                break;
            }
        }
    }

    //WPF: Infragistics.Controls.Layouts.PropertyEditorPropertyDescriptionButtonClickEventHandler
    public void DataGridShowLastHiddenColumn(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var grid = this.grid;
        for (var i = grid.ActualColumns.Count - 1; i >= 0; i--)
        {
            var col = grid.ActualColumns[i];
            if (col.IsHidden)
            {
                col.IsHidden = false;
                break;
            }
        }
    }

    public Random _random = new Random();

    //WPF: Infragistics.Controls.Layouts.PropertyEditorPropertyDescriptionButtonClickEventHandler
    public void DataGridReloadSalaryData(object sender, PropertyEditorPropertyDescriptionButtonClickEventArgs args)
    {
        var grid = this.grid;
        var data = (List<EmployeesSalesDataItem>)grid.ItemsSource;
        for (var i = 0; i < data.Count; i++)
        {
            var item = data[i];
            var oldItem = item;
            item.Salary = Math.Round(60000 + (_random.NextDouble() * 140000));
            grid.NotifySetItem(i, oldItem, item);
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
