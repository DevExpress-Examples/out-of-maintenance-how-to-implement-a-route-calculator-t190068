using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Map;

namespace MapControl_RouteCalculator {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        RoutingHelper helper = new RoutingHelper();
        
        public MainWindow() {
            InitializeComponent();
        }

        #region #EventHandlers
        private void LocationInformationReceived(object sender, LocationInformationReceivedEventArgs e) {
            if ((e.Cancelled) && (e.Result.ResultCode != RequestResultCode.Success)) return;
            GenerateItems(e.Result.Locations);
        }

        private void SearchCompleted(object sender, BingSearchCompletedEventArgs e) {
            if (e.Cancelled || (e.RequestResult.ResultCode != RequestResultCode.Success)) return;

            if (e.RequestResult.SearchResults.Count != 0)
                GenerateItems(e.RequestResult.SearchResults);
            else
                GenerateItems(new LocationInformation[] { e.RequestResult.SearchRegion });
        }

        private void RouteCalculated(object sender, BingRouteCalculatedEventArgs e) {
            if (e.Cancelled) return;

            if (e.CalculationResult.ResultCode != RequestResultCode.Success) return;
            helper.BuildRoute(e.CalculationResult.RouteResults[0].RoutePath);
            UpdateStorage();
        }

        void GenerateItems(IEnumerable<LocationInformation> locations) {
            UpdateStorage();
            foreach (var location in locations) {
                MapPushpin pushpin = new MapPushpin() {
                    Location = location.Location,
                    Information = location
                };
                pushpin.MouseLeftButtonDown += pushpin_MouseLeftButtonDown;
                storage.Items.Add(pushpin);
            }
        }

        void UpdateStorage() {
            storage.Items.Clear();
            storage.Items.Add(helper.Route);
            foreach (MapPushpin pushpin in helper.Pushpins)
                storage.Items.Add(pushpin);
        }
        
        void pushpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            MapPushpin pushpin = sender as MapPushpin;
            if (pushpin == null) return;
            e.Handled = true;
            helper.AddItem(pushpin);
            routeProvider.CalculateRoute(helper.Waypoints);
            pushpin.MouseLeftButtonDown -= pushpin_MouseLeftButtonDown;
        }
        #endregion #EventHandlers

        private void Button_Click(object sender, RoutedEventArgs e) {
            helper.Clear();
            UpdateStorage();
        }
    }

    #region #Helper
    class RoutingHelper {
        List<MapPushpin> pushpins = new List<MapPushpin>();
        MapPolyline route = new MapPolyline() {
            Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0x8A, 0xFB, 0xFF)),
            StrokeStyle = new StrokeStyle() { Thickness = 3 }
        };
        List<RouteWaypoint> waypoints = new List<RouteWaypoint>();
        char currentLatter = 'A';

        public List<MapPushpin> Pushpins { get { return pushpins; } }
        public List<RouteWaypoint> Waypoints { get { return waypoints; } }
        public MapPolyline Route { get { return route; } set { route = value; } }
        public int Count { get { return pushpins.Count; } }

        public void BuildRoute(IEnumerable<GeoPoint> points) {
            route.Points.Clear();
            foreach (GeoPoint point in points)
                route.Points.Add(point);
        }

        public void AddItem(MapPushpin pushpin) {
            pushpins.Add(pushpin);
            waypoints.Add(new RouteWaypoint(
                ((LocationInformation)pushpin.Information).DisplayName,
                (GeoPoint)pushpin.Location)
            );
            pushpin.Text = (currentLatter++).ToString();
        }

        public void Clear() {
            route.Points.Clear();
            pushpins.Clear();
            waypoints.Clear();
            currentLatter = 'A';
        }
    }
    #endregion #Helper
}
