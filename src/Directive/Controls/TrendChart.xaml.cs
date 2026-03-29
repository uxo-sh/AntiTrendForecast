using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AntiTrendForecast.Directive.Controls;

public partial class TrendChart : UserControl
{
    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register("Points", typeof(IEnumerable<double>), typeof(TrendChart), 
            new PropertyMetadata(null, OnPointsChanged));

    public IEnumerable<double> Points
    {
        get => (IEnumerable<double>)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public TrendChart()
    {
        InitializeComponent();
    }

    private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TrendChart chart)
        {
            chart.Redraw();
        }
    }

    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Redraw();
    }

    private void Redraw()
    {
        if (Points == null || !Points.Any() || ChartCanvas.ActualWidth <= 0 || ChartCanvas.ActualHeight <= 0)
        {
            TrendPolyline.Points.Clear();
            AreaPolygon.Points.Clear();
            return;
        }

        var list = Points.ToList();
        // We want the most recent data on the right, so we reverse if the history came in DESC order
        // (Assume input is chronological for now, or handle appropriately)
        
        double width = ChartCanvas.ActualWidth;
        double height = ChartCanvas.ActualHeight;

        var polyPoints = new PointCollection();
        var areaPoints = new PointCollection();

        double stepX = list.Count > 1 ? width / (list.Count - 1) : width;

        // Bottom-left and Bottom-right for the area fill
        areaPoints.Add(new Point(0, height));

        for (int i = 0; i < list.Count; i++)
        {
            // Normalize fatigue (0-100) to pixel height (top = 0, bottom = height)
            double x = i * stepX;
            double y = height - (list[i] / 100.0 * height);
            
            // Clamp to canvas
            y = Math.Clamp(y, 0, height);

            var p = new Point(x, y);
            polyPoints.Add(p);
            areaPoints.Add(p);
        }

        areaPoints.Add(new Point(width, height));

        TrendPolyline.Points = polyPoints;
        AreaPolygon.Points = areaPoints;
    }
}
