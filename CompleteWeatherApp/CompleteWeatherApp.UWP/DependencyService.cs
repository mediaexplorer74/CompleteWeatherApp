using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Xamarin.Forms;
using System.Diagnostics;
using System.ServiceModel;

using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using Windows.Devices.Geolocation; // !

using static CompleteWeatherApp.MainPage;
using System.Threading;


[assembly: Dependency(typeof(CompleteWeatherApp.UWP.GeoInfo))]
namespace CompleteWeatherApp.UWP
{


    // 3 IGeoInfo "interface realization"
    public class GeoInfo : IGeoInfo
    {

        // Proides access to location data
        private Geolocator _geolocator = null;

        // our super client (see details of realization in MegaLib project)
        
        //public static GeoInfo geoinfo = new GeoInfo();

        private static double Latitude = 0;
        private static double Longitude = 0;
        private static int GPSStatus = 0;


        // GetInfo: Say Hello to Service =)
        public string GetInfo()
        {
            // TODO: write some description of service's interface 
            return $"GeoInfo Service";
        }//GetInfo


        //GetStatus
        public int GetStatus()
        {
            return GPSStatus;
        }//GetStatus

        //GetLatitude
        public double GetLatitude()
        {
            // Dirty "Moscow hack" if GPS off! =)
            if (Latitude == 0)
            {
                Latitude = 55.75222;
            }

            return Latitude;
        }//GetLongitude

        //GetLongitude
        public double GetLongitude()
        {
            // Dirty "Moscow hack" if GPS off! =)
            if (Longitude == 0)
            {
                Longitude = 37.61556;
            }

            return Longitude;
        }//GetLongitude


        
        // StartTracking
        //private async void StartTracking(object sender, RoutedEventArgs e)
        public async Task<bool> StartTracking()
        {
            bool Flag = true;

            // Request permission to access location
            //GeolocationAccessStatus accessStatus;// = null;

            try
            {
                var accessStatus = Geolocator.RequestAccessAsync();  // await

               
                await Task.Delay(5000);

                Debug.WriteLine("[i] " + accessStatus.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GPS: Error! GeolocationAccessStatus denied ? : " + ex.Message);

                Flag = false;

                return Flag;
            }

            

            try
            { 
                // Value of 2000 milliseconds (2 seconds) 
                // isn't a requirement, it is just an example.
                _geolocator = new Geolocator { ReportInterval = 2000 };

                // Subscribe to PositionChanged event to get updated tracking positions
                _geolocator.PositionChanged += OnPositionChanged;

                // Subscribe to StatusChanged event to get updates of location status changes
                _geolocator.StatusChanged += OnStatusChanged;
            }
            catch (Exception ex2)
            {
               
                Debug.WriteLine("GPS: Error! GeolocationAccessStatus denied ? Check app permissions : " 
                    + ex2.Message);

                Flag = false;
            }

            return Flag;
            
        }


        // StopTracking
        public bool StopTracking()
        {
            if (_geolocator != null)
            {
                _geolocator.PositionChanged -= OnPositionChanged;
                _geolocator.StatusChanged -= OnStatusChanged;
                _geolocator = null;

                //StartTrackingButton.IsEnabled = true;
                //StopTrackingButton.IsEnabled = false;

                // Clear status
                Debug.WriteLine("GPS: Status cleared!");
            }

            return true;
        }


        // 
        private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    Debug.WriteLine("Location updated.");
                 UpdateLocationData(e.Position);
            //});
        }


        // Event handler for StatusChanged events. It is raised when the 
        // location status in the system changes.
        private async void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            //await System.ServiceModel.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
                // Show the location setting message only if status is disabled.
                //LocationDisabledMessage.Visibility = Visibility.Collapsed;

                switch (e.Status)
                {
                    case PositionStatus.Ready:
                    // Location platform is providing valid data.
                    //ScenarioOutput_Status.Text = "Ready";
                    GPSStatus = 5; // "OK"
                        Debug.WriteLine("GPS : Location platform is ready.");
                        break;

                    case PositionStatus.Initializing:
                    // Location platform is attempting to acquire a fix. 
                    //ScenarioOutput_Status.Text = "Initializing";
                    GPSStatus = 4; // "Initializing"
                    Debug.WriteLine("GPS: Location platform is attempting to obtain a position.");
                        break;

                    case PositionStatus.NoData:
                    // Location platform could not obtain location data.
                    //ScenarioOutput_Status.Text = "No data";
                    GPSStatus = 3; // "Not able to determine the location"
                    Debug.WriteLine("GPS: Not able to determine the location.");
                        break;

                    case PositionStatus.Disabled:
                    // The permission to access location data is denied by the user or other policies.
                    //ScenarioOutput_Status.Text = "Disabled";
                    GPSStatus = 2; // "Disabled"
                    Debug.WriteLine("GPS: Access to location is denied.");

                        // Show message to the user to go to location settings
                        //LocationDisabledMessage.Visibility = Visibility.Visible;

                        // Clear cached location data if any
                        UpdateLocationData(null);
                        break;

                    case PositionStatus.NotInitialized:
                    // The location platform is not initialized. This indicates that the application 
                    // has not made a request for location data.
                    //ScenarioOutput_Status.Text = "Not initialized";
                    GPSStatus = 1; // "Not initialized"
                    Debug.WriteLine("No request for location is made yet.");
                        break;

                    case PositionStatus.NotAvailable:
                    // The location platform is not available on this version of the OS.
                    //ScenarioOutput_Status.Text = "Not available";
                    GPSStatus = 10; // "Service not available on this version of the OS"
                    Debug.WriteLine("Location is not available on this version of the OS.");
                        break;

                    default:
                    //ScenarioOutput_Status.Text = "Unknown";
                    GPSStatus = 9; // "Unknown"
                    Debug.WriteLine("Unknown");
                        break;
                }
            //});
        }


        // GetGeoInfo
        private async Task GetGeoInfo()
        {
            //bool Flag = false;

            GeolocationAccessStatus accessStatus;// = null;

            try
            {
                accessStatus = await Geolocator.RequestAccessAsync();
            

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:
                        // Create Geolocator and define perodic-based tracking (2 second interval).
                        _geolocator = new Geolocator { ReportInterval = 2000 };

                        // Subscribe to the PositionChanged event to get location updates.
                        _geolocator.PositionChanged += OnPositionChanged;

                        // Subscribe to StatusChanged event to get updates of location status changes.
                        _geolocator.StatusChanged += OnStatusChanged;

                        Debug.WriteLine("Waiting for update...");

                        //LocationDisabledMessage.Visibility = Visibility.Collapsed;
                        //StartTrackingButton.IsEnabled = false;
                        //StopTrackingButton.IsEnabled = true;

                        //Flag = true;
                        break;

                    case GeolocationAccessStatus.Denied:

                        Debug.WriteLine("Access to location is denied.");

                        //LocationDisabledMessage.Visibility = Visibility.Visible;
                        break;

                    case GeolocationAccessStatus.Unspecified:

                        Debug.WriteLine("Unspecificed error!");

                        //LocationDisabledMessage.Visibility = Visibility.Collapsed;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }

            //return Flag;
        }//GetGeoInfo


       private void UpdateLocationData(Geoposition position)
        {
            if (position == null)
            {
                //ScenarioOutput_Latitude.Text = "No data";
                //ScenarioOutput_Longitude.Text = "No data";
                //ScenarioOutput_Accuracy.Text = "No data";
            }
            else
            {
                Latitude = position.Coordinate.Point.Position.Latitude;
                Longitude = position.Coordinate.Point.Position.Longitude;
                //ScenarioOutput_Latitude.Text = position.Coordinate.Point.Position.Latitude.ToString();
                //ScenarioOutput_Longitude.Text = position.Coordinate.Point.Position.Longitude.ToString();
                //ScenarioOutput_Accuracy.Text = position.Coordinate.Accuracy.ToString();
            }
        }


    }//GeoInfo : IGeoInfo


}
