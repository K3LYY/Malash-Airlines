using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Malash_Airlines
{
    public partial class FlightMapVisualization : Window
    {
        private Flight flight;

        private static readonly Dictionary<string, (double Lat, double Lon)> AirportLocations = new()
        {
            { "Lotnisko Chopina", (52.1657, 20.9671) },      // Warsaw
            { "Heathrow Airport", (51.4700, -0.4543) },      // London
            { "JFK International Airport", (40.6413, -73.7781) }, // New York
            { "Charles de Gaulle Airport", (49.0097, 2.5479) },  // Paris
            { "Frankfurt Airport", (50.0333, 8.5706) }       // Frankfurt
        };

        public FlightMapVisualization(Flight flightData)
        {
            InitializeComponent();
            flight = flightData;
            this.Title = $"Flight Map: {flight.Departure} to {flight.Destination}";
            DrawFlightMap();
        }

        private void DrawFlightMap()
        {
            mapCanvas.Children.Clear();

            Rectangle mapBackground = new Rectangle
            {
                Width = mapCanvas.Width,
                Height = mapCanvas.Height,
                Fill = new SolidColorBrush(Color.FromRgb(240, 248, 255))
            };
            mapCanvas.Children.Add(mapBackground);

            Path worldOutline = new Path
            {
                Stroke = new SolidColorBrush(Color.FromRgb(169, 169, 169)),
                StrokeThickness = 1,
                Data = CreateWorldMap()
            };
            mapCanvas.Children.Add(worldOutline);

            Point departureLocation = GetAirportLocation(flight.Departure);
            Point destinationLocation = GetAirportLocation(flight.Destination);

            DrawAirport(departureLocation, Colors.Green);
            DrawAirport(destinationLocation, Colors.Red);
            DrawFlightPath(departureLocation, destinationLocation);
            DrawLabels(departureLocation, destinationLocation);
            DrawLegend();
            DrawFlightInfo();
        }

        private Point GetAirportLocation(string airportName)
        {
            if (AirportLocations.TryGetValue(airportName, out var coords))
            {
                return ConvertLatLonToCanvas(coords.Lat, coords.Lon);
            }
            return new Point(mapCanvas.Width / 2, mapCanvas.Height / 2);
        }

        private Point ConvertLatLonToCanvas(double lat, double lon)
        {
            const double minLat = -60, maxLat = 80;
            const double minLon = -180, maxLon = 180;

            double xNormalized = (lon - minLon) / (maxLon - minLon);
            double yNormalized = (maxLat - lat) / (maxLat - minLat);

            double x = xNormalized * mapCanvas.Width;
            double y = yNormalized * mapCanvas.Height;

            return new Point(x, y);
        }

        private void DrawAirport(Point location, Color color)
        {
            Ellipse airportMarker = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(color)
            };
            Canvas.SetLeft(airportMarker, location.X - 5);
            Canvas.SetTop(airportMarker, location.Y - 5);
            mapCanvas.Children.Add(airportMarker);
        }

        private void DrawFlightPath(Point start, Point end)
        {
            Path flightPath = new Path
            {
                Stroke = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 2 }
            };

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = start };

            Point midPoint = new Point((start.X + end.X) / 2, Math.Min(start.Y, end.Y) - 30);

            BezierSegment bezierSegment = new BezierSegment(
                new Point((2 * start.X + end.X) / 3, midPoint.Y),
                new Point((start.X + 2 * end.X) / 3, midPoint.Y),
                end,
                true);

            figure.Segments.Add(bezierSegment);
            geometry.Figures.Add(figure);
            flightPath.Data = geometry;
            mapCanvas.Children.Add(flightPath);
        }

        private void DrawLabels(Point departure, Point destination)
        {
            AddLabel(flight.Departure, departure, 10, -10);
            AddLabel(flight.Destination, destination, -60, -10);
        }

        private void AddLabel(string text, Point location, double offsetX, double offsetY)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(label, location.X + offsetX);
            Canvas.SetTop(label, location.Y + offsetY);
            mapCanvas.Children.Add(label);
        }

        private Geometry CreateWorldMap()
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                // North America (simplified)
                context.BeginFigure(new Point(50, 100), false, false);
                context.LineTo(new Point(150, 80), true, false);
                context.LineTo(new Point(180, 150), true, false);
                context.LineTo(new Point(120, 200), true, false);
                context.LineTo(new Point(50, 100), true, false);

                // South America (simplified)
                context.BeginFigure(new Point(120, 200), false, false);
                context.LineTo(new Point(150, 300), true, false);
                context.LineTo(new Point(100, 350), true, false);
                context.LineTo(new Point(80, 250), true, false);
                context.LineTo(new Point(120, 200), true, false);

                // Europe (simplified)
                context.BeginFigure(new Point(250, 100), false, false);
                context.LineTo(new Point(300, 80), true, false);
                context.LineTo(new Point(320, 120), true, false);
                context.LineTo(new Point(280, 150), true, false);
                context.LineTo(new Point(250, 100), true, false);

                // Africa (simplified)
                context.BeginFigure(new Point(280, 150), false, false);
                context.LineTo(new Point(320, 120), true, false);
                context.LineTo(new Point(350, 250), true, false);
                context.LineTo(new Point(300, 300), true, false);
                context.LineTo(new Point(250, 200), true, false);
                context.LineTo(new Point(280, 150), true, false);

                // Asia (simplified)
                context.BeginFigure(new Point(320, 120), false, false);
                context.LineTo(new Point(450, 80), true, false);
                context.LineTo(new Point(500, 150), true, false);
                context.LineTo(new Point(450, 200), true, false);
                context.LineTo(new Point(350, 250), true, false);
                context.LineTo(new Point(320, 120), true, false);

                // Australia (simplified)
                context.BeginFigure(new Point(450, 250), false, false);
                context.LineTo(new Point(500, 250), true, false);
                context.LineTo(new Point(500, 300), true, false);
                context.LineTo(new Point(450, 300), true, false);
                context.LineTo(new Point(450, 250), true, false);
            }

            return geometry;

        }

        private void DrawLegend()
        {
            // Create legend background
            Rectangle legendBackground = new Rectangle
            {
                Width = 150,
                Height = 80,
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetRight(legendBackground, 10);
            Canvas.SetBottom(legendBackground, 10);
            mapCanvas.Children.Add(legendBackground);

            // Legend title
            TextBlock legendTitle = new TextBlock
            {
                Text = "Legend",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(legendTitle, 105);
            Canvas.SetBottom(legendTitle, 65);
            mapCanvas.Children.Add(legendTitle);

            // Departure indicator
            Ellipse departureIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(46, 204, 113)) // Green
            };
            Canvas.SetRight(departureIndicator, 140);
            Canvas.SetBottom(departureIndicator, 45);
            mapCanvas.Children.Add(departureIndicator);

            TextBlock departureText = new TextBlock
            {
                Text = "Departure",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(departureText, 20);
            Canvas.SetBottom(departureText, 42);
            mapCanvas.Children.Add(departureText);

            // Destination indicator
            Ellipse destinationIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60)) // Red
            };
            Canvas.SetRight(destinationIndicator, 140);
            Canvas.SetBottom(destinationIndicator, 25);
            mapCanvas.Children.Add(destinationIndicator);

            TextBlock destinationText = new TextBlock
            {
                Text = "Destination",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(destinationText, 20);
            Canvas.SetBottom(destinationText, 22);
            mapCanvas.Children.Add(destinationText);
        }



        private void DrawFlightInfo()
        {
            // Create flight info background
            Rectangle infoBackground = new Rectangle
            {
                Width = 200,
                Height = 100,
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(infoBackground, 10);
            Canvas.SetTop(infoBackground, 10);
            mapCanvas.Children.Add(infoBackground);

            // Flight info title
            TextBlock infoTitle = new TextBlock
            {
                Text = "Flight Information",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(infoTitle, 20);
            Canvas.SetTop(infoTitle, 15);
            mapCanvas.Children.Add(infoTitle);

            // Flight ID
            TextBlock flightId = new TextBlock
            {
                Text = $"Flight #: {flight.ID}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(flightId, 20);
            Canvas.SetTop(flightId, 35);
            mapCanvas.Children.Add(flightId);

            // Flight date
            TextBlock flightDate = new TextBlock
            {
                Text = $"Date: {flight.Date.ToString("MM/dd/yyyy")}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(flightDate, 20);
            Canvas.SetTop(flightDate, 55);
            mapCanvas.Children.Add(flightDate);

            // Flight time
            TextBlock flightTime = new TextBlock
            {
                Text = $"Time: {flight.Time}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(flightTime, 20);
            Canvas.SetTop(flightTime, 75);
            mapCanvas.Children.Add(flightTime);
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
