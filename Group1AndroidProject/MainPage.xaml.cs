using Mapsui.Projections;
using Mapsui.UI.Maui;
using Mapsui.Tiling;

namespace Group1AndroidProject
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        public MapControl mapView;

        public MainPage()
        {
            InitializeComponent();
            InitializeMap();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
        }
        private void ToggleButton_Clicked(object sender, EventArgs e)
        {

        }
        private async void InitializeMap()
        {
            mapView = new MapControl();
            // Set up the base map (OpenStreetMap)
            var tileLayer = OpenStreetMap.CreateTileLayer();
            mapView.Map?.Layers.Add(tileLayer);

            // Check and request location permission
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best
                });
            }

            if (location != null)
            {
                // Convert GPS coordinates to Mapsui format and set the center
                var position = SphericalMercator.FromLonLat(location.Longitude, location.Latitude);
                mapView.Map?.Navigator.CenterOn(position.x,position.y);
            }
            else
            {
                await DisplayAlert("Error", "Unable to retrieve location.", "OK");
            }
        }
    }

}
