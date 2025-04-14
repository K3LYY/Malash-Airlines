using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup;
using System.Globalization;
using Path = System.Windows.Shapes.Path;
using IOPath = System.IO.Path;

namespace Malash_Airlines
{
    public partial class FlightMapVisualization : Window
    {
        private Flight flight;

        private static readonly Dictionary<string, (double Lat, double Lon, string Code)> AirportLocations = new()
        {
            // Europe
{ "Lotnisko Chopina", (52.1657, 20.9671, "WAW") },                // Warszawa, Polska
{ "Lotnisko Heathrow", (51.4700, -0.4543, "LHR") },                // Londyn, Wielka Brytania
{ "Lotnisko Charles de Gaulle", (49.0097, 2.5479, "CDG") },        // Paryż, Francja
{ "Lotnisko frankfurckie", (50.0333, 8.5706, "FRA") },              // Frankfurt, Niemcy
{ "Lotnisko Schiphol", (52.3105, 4.7683, "AMS") },                  // Amsterdam, Holandia
{ "Lotnisko Madrid-Barajas", (40.4983, -3.5676, "MAD") },           // Madryt, Hiszpania
{ "Lotnisko Leonardo da Vinci", (41.8045, 12.2508, "FCO") },        // Rzym, Włochy
{ "Lotnisko w Istambule", (41.2608, 28.7418, "IST") },              // Stambuł, Turcja
{ "Lotnisko monachijskie", (48.3538, 11.7861, "MUC") },             // Monachium, Niemcy
{ "Międzynarodowe Lotnisko Wiedeń", (48.1103, 16.5697, "VIE") },      // Wiedeń, Austria
{ "Lotnisko w Zurychu", (47.4647, 8.5492, "ZRH") },                 // Zurych, Szwajcaria
{ "Lotnisko Barcelona-El Prat", (41.2974, 2.0833, "BCN") },         // Barcelona, Hiszpania

// Ameryka Północna
{ "Międzynarodowe Lotnisko JFK", (40.6413, -73.7781, "JFK") },       // Nowy Jork, USA
{ "Międzynarodowe Lotnisko Los Angeles", (33.9416, -118.4085, "LAX") },// Los Angeles, USA
{ "Międzynarodowe Lotnisko O'Hare", (41.9742, -87.9073, "ORD") },     // Chicago, USA
{ "Lotnisko Toronto Pearson", (43.6777, -79.6248, "YYZ") },         // Toronto, Kanada
{ "Lotnisko Dallas/Fort Worth", (32.8998, -97.0403, "DFW") },         // Dallas, USA
{ "Międzynarodowe Lotnisko Denver", (39.8561, -104.6737, "DEN") },     // Denver, USA
{ "Lotnisko Hartsfield-Jackson Atlanta", (33.6407, -84.4277, "ATL") }, // Atlanta, USA
{ "Lotnisko Miami International", (25.7933, -80.2906, "MIA") },       // Miami, USA
{ "Międzynarodowe Lotnisko San Francisco", (37.6213, -122.3790, "SFO") }, // San Francisco, USA
{ "Lotnisko Vancouver International", (49.1967, -123.1815, "YVR") },  // Vancouver, Kanada
{ "Lotnisko w Mexico City", (19.4363, -99.0721, "MEX") },             // Mexico City, Meksyk

// Azja
{ "Lotnisko w Pekinie", (40.0799, 116.6031, "PEK") },                // Pekin, Chiny
{ "Międzynarodowe Lotnisko Dubaj", (25.2528, 55.3644, "DXB") },        // Dubaj, ZEA
{ "Lotnisko Tokyo Haneda", (35.5494, 139.7798, "HND") },              // Tokio, Japonia
{ "Lotnisko Singapore Changi", (1.3644, 103.9915, "SIN") },           // Singapur
{ "Międzynarodowe Lotnisko Incheon", (37.4602, 126.4407, "ICN") },     // Seul, Korea Południowa
{ "Międzynarodowe Lotnisko Indira Gandhi", (28.5561, 77.1000, "DEL") }, // Delhi, Indie
{ "Międzynarodowe Lotnisko Hong Kong", (22.3080, 113.9185, "HKG") },   // Hongkong
{ "Lotnisko Bangkok Suvarnabhumi", (13.6900, 100.7501, "BKK") },       // Bangkok, Tajlandia
{ "Międzynarodowe Lotnisko Kuala Lumpur", (2.7456, 101.7099, "KUL") },  // Kuala Lumpur, Malezja
{ "Międzynarodowe Lotnisko Doha Hamad", (25.2609, 51.6138, "DOH") },    // Doha, Katar

// Pozostałe regiony
{ "Lotnisko Sydney Kingsford Smith", (-33.9399, 151.1753, "SYD") },  // Sydney, Australia
{ "Lotnisko Melbourne", (-37.6690, 144.8410, "MEL") },               // Melbourne, Australia
{ "Lotnisko Auckland", (-37.0082, 174.7850, "AKL") },                // Auckland, Nowa Zelandia
{ "Lotnisko Johannesburg OR Tambo", (-26.1367, 28.2411, "JNB") },      // Johannesburg, Republika Południowej Afryki
{ "Lotnisko São Paulo – Guarulhos", (-23.4356, -46.4731, "GRU") },     // São Paulo, Brazylia
{ "Międzynarodowe Lotnisko Kair", (30.1219, 31.4056, "CAI") },         // Kair, Egipt
{ "Międzynarodowe Lotnisko Cape Town", (-33.9715, 18.6021, "CPT") },    // Cape Town, Republika Południowej Afryki
{ "Lotnisko Rio de Janeiro-Galeão", (-22.8099, -43.2506, "GIG") }       // Rio de Janeiro, Brazylia

        };

        private const double SvgMinLon = -169.110266;
        private const double SvgMaxLat = 83.600842;
        private const double SvgMaxLon = 190.486279;
        private const double SvgMinLat = -58.508473;
        private const double SvgWidth = 1009.6727;
        private const double SvgHeight = 665.96301;

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

            DrawSvgWorldMap();

            Point departureLocation = GetAirportLocation(flight.Departure);
            Point destinationLocation = GetAirportLocation(flight.Destination);

            DrawAllAirports();
            
            DrawAirport(departureLocation, Colors.Green, 12);
            DrawAirport(destinationLocation, Colors.Red, 12);
            
            DrawFlightPath(departureLocation, destinationLocation);
            DrawLabels(departureLocation, destinationLocation);
            DrawLegend();
            DrawFlightInfo();
        }

        private void DrawSvgWorldMap() {
            try {
                string[] possiblePaths = new string[]
                {
                    IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "world.svg"),
                    IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "world.svg"),
                    IOPath.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "world.svg"),
                    "world.svg"
                };

                string svgFilePath = null;
                foreach (var path in possiblePaths) {
                    if (System.IO.File.Exists(path)) {
                        svgFilePath = path;
                        break;
                    }
                }

                if (svgFilePath == null) {
                    MessageBox.Show("World SVG map file not found. Using simplified map instead.", "Map Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Path worldOutline = new Path {
                        Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                        StrokeThickness = 0.8,
                        Data = CreateWorldMap()
                    };
                    mapCanvas.Children.Add(worldOutline);
                    return;
                }

                string svgContent = System.IO.File.ReadAllText(svgFilePath);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(svgContent);

                Canvas mapGroup = new Canvas {
                    Width = mapCanvas.Width,
                    Height = mapCanvas.Height
                };

                double scaleX = mapCanvas.Width / SvgWidth;
                double scaleY = mapCanvas.Height / SvgHeight;
                double baseScale = Math.Min(scaleX, scaleY);
                double newScale = baseScale * 1;
                double translateX = (mapCanvas.Width - SvgWidth * newScale) / 2;
                double translateY = (mapCanvas.Height - SvgHeight * newScale) / 2;

                XmlNodeList pathNodes = doc.GetElementsByTagName("path");

                foreach (XmlNode pathNode in pathNodes) {
                    string pathData = pathNode.Attributes["d"]?.Value;
                    string title = pathNode.Attributes["title"]?.Value ?? "Country";
                    string id = pathNode.Attributes["id"]?.Value ?? "";

                    if (!string.IsNullOrEmpty(pathData)) {
                        Geometry geometry = ParseSvgPath(pathData);

                        if (geometry != null) {
                            Path countryPath = new Path {
                                Data = geometry,
                                Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                                StrokeThickness = 0.5,
                                Fill = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                                ToolTip = title,
                                Tag = id
                            };

                            TransformGroup transforms = new TransformGroup();
                            transforms.Children.Add(new ScaleTransform(newScale, newScale));
                            transforms.Children.Add(new TranslateTransform(translateX, translateY));
                            countryPath.RenderTransform = transforms;

                            mapGroup.Children.Add(countryPath);
                        }
                    }
                }

                mapCanvas.Children.Add(mapGroup);
            } catch (Exception ex) {
                MessageBox.Show($"Error loading SVG map: {ex.Message}", "Map Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Path worldOutline = new Path {
                    Stroke = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                    StrokeThickness = 0.8,
                    Data = CreateWorldMap()
                };
                mapCanvas.Children.Add(worldOutline);
            }
        }


        private Geometry ParseSvgPath(string pathData)
        {
            try
            {
                string xamlPath = $"<Path xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Data=\"{pathData}\"/>";
                Path path = XamlReader.Parse(xamlPath) as Path;
                return path?.Data;
            }
            catch
            {
                StreamGeometry streamGeometry = new StreamGeometry();

                using (StreamGeometryContext ctx = streamGeometry.Open())
                {
                    bool isFirstCommand = true;
                    int i = 0;

                    while (i < pathData.Length)
                    {
                        while (i < pathData.Length && char.IsWhiteSpace(pathData[i]))
                            i++;

                        if (i >= pathData.Length)
                            break;

                        char command = pathData[i];
                        i++;

                        while (i < pathData.Length && char.IsWhiteSpace(pathData[i]))
                            i++;

                        switch (command)
                        {
                            case 'm':
                            case 'M':
                                if (TryParsePoint(pathData, ref i, out Point point))
                                {
                                    ctx.BeginFigure(point, true, false);
                                    isFirstCommand = false;
                                }
                                break;
                            case 'l':
                            case 'L':
                                if (!isFirstCommand && TryParsePoint(pathData, ref i, out Point linePoint))
                                {
                                    ctx.LineTo(linePoint, true, true);
                                }
                                break;
                            case 'z':
                            case 'Z':
                                ctx.Close();
                                break;
                            case 'c':
                            case 'C':
                                if (!isFirstCommand && 
                                    TryParsePoint(pathData, ref i, out Point controlPoint1) && 
                                    TryParsePoint(pathData, ref i, out Point controlPoint2) && 
                                    TryParsePoint(pathData, ref i, out Point endPoint))
                                {
                                    ctx.BezierTo(controlPoint1, controlPoint2, endPoint, true, true);
                                }
                                break;
                            default:
                                while (i < pathData.Length && !char.IsLetter(pathData[i]))
                                    i++;
                                break;
                        }
                    }
                }

                return streamGeometry;
            }
        }

        private bool TryParsePoint(string pathData, ref int index, out Point point)
        {
            point = new Point();
            string xStr = "", yStr = "";

            while (index < pathData.Length && char.IsWhiteSpace(pathData[index]))
                index++;

            while (index < pathData.Length && 
                  (char.IsDigit(pathData[index]) || pathData[index] == '.' || pathData[index] == '-' || 
                   pathData[index] == 'e' || pathData[index] == 'E' || pathData[index] == '+'))
            {
                xStr += pathData[index];
                index++;
            }

            while (index < pathData.Length && (char.IsWhiteSpace(pathData[index]) || pathData[index] == ','))
                index++;

            while (index < pathData.Length && 
                  (char.IsDigit(pathData[index]) || pathData[index] == '.' || pathData[index] == '-' || 
                   pathData[index] == 'e' || pathData[index] == 'E' || pathData[index] == '+'))
            {
                yStr += pathData[index];
                index++;
            }

            if (double.TryParse(xStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double x) &&
                double.TryParse(yStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
            {
                point = new Point(x, y);
                return true;
            }

            return false;
        }

        private Point GetAirportLocation(string airportName)
        {
            if (AirportLocations.TryGetValue(airportName, out var coords))
            {
                return ConvertLatLonToCanvas(coords.Lat, coords.Lon);
            }
            return new Point(mapCanvas.Width / 2, mapCanvas.Height / 2);
        }

        private Point ConvertLatLonToCanvas(double lat, double lon) {
            double scale = Math.Min(mapCanvas.Width / SvgWidth, mapCanvas.Height / SvgHeight);
            double translateX = (mapCanvas.Width - SvgWidth * scale) / 2;
            double translateY = (mapCanvas.Height - SvgHeight * scale) / 2;

            double xNormalized = (lon - SvgMinLon) / (SvgMaxLon - SvgMinLon);
            double yNormalized = (SvgMaxLat - lat) / (SvgMaxLat - SvgMinLat);

            double offsetY = 95;

            double x = translateX + (xNormalized * SvgWidth * scale);
            double y = translateY + (yNormalized * SvgHeight * scale) + offsetY;

            return new Point(x, y);
        }


        private void DrawAllAirports()
        {
            foreach (var airport in AirportLocations)
            {
                if (airport.Key == flight.Departure || airport.Key == flight.Destination)
                    continue;
                
                Point location = ConvertLatLonToCanvas(airport.Value.Lat, airport.Value.Lon);
                DrawAirport(location, Colors.DarkGray, 5);
                
                TextBlock airportCode = new TextBlock
                {
                    Text = airport.Value.Code,
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Colors.DarkGray)
                };
                Canvas.SetLeft(airportCode, location.X + 6);
                Canvas.SetTop(airportCode, location.Y - 3);
                mapCanvas.Children.Add(airportCode);
            }
        }

        private void DrawAirport(Point location, Color color, double size = 10)
        {
            Ellipse airportMarker = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1
            };
            Canvas.SetLeft(airportMarker, location.X - size/2);
            Canvas.SetTop(airportMarker, location.Y - size/2);
            mapCanvas.Children.Add(airportMarker);
            
            if (size > 8)
            {
                Ellipse pulse = new Ellipse
                {
                    Width = size * 2,
                    Height = size * 2,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1.5,
                    Fill = new SolidColorBrush(Color.FromArgb(50, color.R, color.G, color.B))
                };
                Canvas.SetLeft(pulse, location.X - size);
                Canvas.SetTop(pulse, location.Y - size);
                mapCanvas.Children.Add(pulse);
            }
        }

        private void DrawFlightPath(Point start, Point end)
        {
            double distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            double curveHeight = Math.Min(distance * 0.15, 60);
            
            Path flightPath = new Path
            {
                Stroke = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                StrokeThickness = 2.5,
                StrokeDashArray = new DoubleCollection { 5, 2 }
            };

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure { StartPoint = start };

            Point midPoint = new Point(
                (start.X + end.X) / 2, 
                Math.Min(start.Y, end.Y) - curveHeight
            );

            BezierSegment bezierSegment = new BezierSegment(
                new Point((2 * start.X + end.X) / 3, midPoint.Y),
                new Point((start.X + 2 * end.X) / 3, midPoint.Y),
                end,
                true);

            figure.Segments.Add(bezierSegment);
            geometry.Figures.Add(figure);
            flightPath.Data = geometry;
            mapCanvas.Children.Add(flightPath);
            
            DrawAirplaneIcon(midPoint);
        }
        
        private void DrawAirplaneIcon(Point location)
        {
            Path airplaneIcon = new Path
            {
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                StrokeThickness = 1.5,
                Data = Geometry.Parse("M20,12 L25,17 L23,20 L18,18 L14,25 L11,24 L13,17 L8,15 L6,18 L3,17 L8,12 L3,7 L6,6 L8,9 L13,7 L11,0 L14,-1 L18,6 L23,4 L25,7 L20,12z")
            };
            
            Canvas.SetLeft(airplaneIcon, location.X - 14);
            Canvas.SetTop(airplaneIcon, location.Y - 12);
            
            RotateTransform rotateTransform = new RotateTransform(45);
            airplaneIcon.RenderTransform = rotateTransform;
            
            mapCanvas.Children.Add(airplaneIcon);
        }

        private void DrawLabels(Point departure, Point destination)
        {
            double dOffsetX = departure.X < mapCanvas.Width / 2 ? 10 : -60;
            
            double destOffsetX = destination.X < mapCanvas.Width / 2 ? 10 : -60;
            
            string departureCode = "";
            string destinationCode = "";
            
            foreach (var airport in AirportLocations)
            {
                if (airport.Key == flight.Departure)
                    departureCode = airport.Value.Code;
                if (airport.Key == flight.Destination)
                    destinationCode = airport.Value.Code;
            }
            
            AddLabelWithBackground(flight.Departure, departure, dOffsetX, -15, departureCode);
            AddLabelWithBackground(flight.Destination, destination, destOffsetX, -15, destinationCode);
        }

        private void AddLabelWithBackground(string text, Point location, double offsetX, double offsetY, string code)
        {
            string displayText = $"{text} ({code})";
            
            Rectangle labelBg = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Stroke = new SolidColorBrush(Colors.LightGray),
                StrokeThickness = 1,
                RadiusX = 4,
                RadiusY = 4
            };
            
            TextBlock label = new TextBlock
            {
                Text = displayText,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black),
                Padding = new Thickness(4)
            };
            
            label.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size textSize = label.DesiredSize;
            
            labelBg.Width = textSize.Width + 8;
            labelBg.Height = textSize.Height + 4;
            
            Canvas.SetLeft(labelBg, location.X + offsetX);
            Canvas.SetTop(labelBg, location.Y + offsetY);
            
            Canvas.SetLeft(label, location.X + offsetX + 4);
            Canvas.SetTop(label, location.Y + offsetY);
            
            mapCanvas.Children.Add(labelBg);
            mapCanvas.Children.Add(label);
        }

        private Geometry CreateWorldMap()
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                // Ameryka Północna (uproszczona)
                context.BeginFigure(new Point(50, 100), false, false);
                context.LineTo(new Point(150, 80), true, false);
                context.LineTo(new Point(180, 150), true, false);
                context.LineTo(new Point(120, 200), true, false);
                context.LineTo(new Point(50, 100), true, false);

                // Ameryka Południowa (uproszczona)
                context.BeginFigure(new Point(120, 200), false, false);
                context.LineTo(new Point(150, 300), true, false);
                context.LineTo(new Point(100, 350), true, false);
                context.LineTo(new Point(80, 250), true, false);
                context.LineTo(new Point(120, 200), true, false);

                // Europa (uproszczona)
                context.BeginFigure(new Point(250, 100), false, false);
                context.LineTo(new Point(300, 80), true, false);
                context.LineTo(new Point(320, 120), true, false);
                context.LineTo(new Point(280, 150), true, false);
                context.LineTo(new Point(250, 100), true, false);

                // Afryka (uproszczona)
                context.BeginFigure(new Point(280, 150), false, false);
                context.LineTo(new Point(320, 120), true, false);
                context.LineTo(new Point(350, 250), true, false);
                context.LineTo(new Point(300, 300), true, false);
                context.LineTo(new Point(250, 200), true, false);
                context.LineTo(new Point(280, 150), true, false);

                // Azja (uproszczona)
                context.BeginFigure(new Point(320, 120), false, false);
                context.LineTo(new Point(450, 80), true, false);
                context.LineTo(new Point(500, 150), true, false);
                context.LineTo(new Point(450, 200), true, false);
                context.LineTo(new Point(350, 250), true, false);
                context.LineTo(new Point(320, 120), true, false);

                // Australia (uproszczona)
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
            Rectangle legendBackground = new Rectangle
            {
                Width = 170,
                Height = 110,
                Fill = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetRight(legendBackground, 10);
            Canvas.SetBottom(legendBackground, 10);
            mapCanvas.Children.Add(legendBackground);

            TextBlock legendTitle = new TextBlock
            {
                Text = "Flight Legend",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(legendTitle, 105);
            Canvas.SetBottom(legendTitle, 90);
            mapCanvas.Children.Add(legendTitle);

            Ellipse departureIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(46, 204, 113))
            };
            Canvas.SetRight(departureIndicator, 150);
            Canvas.SetBottom(departureIndicator, 70);
            mapCanvas.Children.Add(departureIndicator);

            TextBlock departureText = new TextBlock
            {
                Text = "Departure",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(departureText, 30);
            Canvas.SetBottom(departureText, 67);
            mapCanvas.Children.Add(departureText);

            Ellipse destinationIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60))
            };
            Canvas.SetRight(destinationIndicator, 150);
            Canvas.SetBottom(destinationIndicator, 50);
            mapCanvas.Children.Add(destinationIndicator);

            TextBlock destinationText = new TextBlock
            {
                Text = "Destination",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(destinationText, 30);
            Canvas.SetBottom(destinationText, 47);
            mapCanvas.Children.Add(destinationText);
            
            Ellipse otherAirportsIndicator = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Colors.DarkGray)
            };
            Canvas.SetRight(otherAirportsIndicator, 150);
            Canvas.SetBottom(otherAirportsIndicator, 30);
            mapCanvas.Children.Add(otherAirportsIndicator);

            TextBlock otherAirportsText = new TextBlock
            {
                Text = "Other Airports",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetRight(otherAirportsText, 30);
            Canvas.SetBottom(otherAirportsText, 27);
            mapCanvas.Children.Add(otherAirportsText);
        }

        private void DrawFlightInfo()
        {
            Rectangle infoBackground = new Rectangle
            {
                Width = 200,
                Height = 120,
                Fill = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(infoBackground, 10);
            Canvas.SetTop(infoBackground, 10);
            mapCanvas.Children.Add(infoBackground);

            TextBlock infoTitle = new TextBlock
            {
                Text = "Flight Information",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(infoTitle, 20);
            Canvas.SetTop(infoTitle, 15);
            mapCanvas.Children.Add(infoTitle);

            TextBlock flightId = new TextBlock
            {
                Text = $"Flight #: {flight.ID}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(flightId, 20);
            Canvas.SetTop(flightId, 35);
            mapCanvas.Children.Add(flightId);

            TextBlock flightDate = new TextBlock
            {
                Text = $"Date: {flight.Date.ToString("MM/dd/yyyy")}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(flightDate, 20);
            Canvas.SetTop(flightDate, 55);
            mapCanvas.Children.Add(flightDate);

            TextBlock flightTime = new TextBlock
            {
                Text = $"Time: {flight.Time}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(flightTime, 20);
            Canvas.SetTop(flightTime, 75);
            mapCanvas.Children.Add(flightTime);
            
            TextBlock aircraftInfo = new TextBlock
            {
                Text = $"Aircraft: {flight.Plane}",
                Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(aircraftInfo, 20);
            Canvas.SetTop(aircraftInfo, 95);
            mapCanvas.Children.Add(aircraftInfo);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
