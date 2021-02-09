
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using System.Diagnostics;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using CompleteWeatherApp.Helper;
using CompleteWeatherApp.Models;
using Newtonsoft.Json;

using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace CompleteWeatherApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        // Этот интерфейс будет определять сигнатуру методов, 
        // реализация которых будет зависеть от конкретной платформы. 

        // 1 Interface connector
        IGeoInfo geoInfo;

        // 2 Interface declaration
        public interface IGeoInfo
        {
            string GetInfo();

            int GetStatus();

            bool StartTracking();

            double GetLatitude();

            double GetLongitude();
            
            bool StopTracking();

        }

        public string AppId = "SuperWeatherApp";
        public string APIKey = "8b6d92f6ff72b8a7bacfb35cde4de4e7"; // cf0e69598b69f43df04a877079887687


        public string lon = "0";
        public string lat = "0";

        public MainPage()
        {
            InitializeComponent();

            // !!! Interface Init  
            geoInfo = DependencyService.Get<IGeoInfo>();

            // Try to start geolocation tracking...
            geoInfo.StartTracking();

            while ( (geoInfo.GetStatus() != 2) && (geoInfo.GetStatus() != 5))
            {
                // wait gps's init
            }

            // Stop geolocation tracking

            geoInfo.StopTracking();


            //GetWeatherInfo();
            GetCoordinates();

        }

        private string Location { get; set; } = "Globe"; // All our world, all Earth =)
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Simple GPS tracking =)
        private async void GetCoordinates()
        {

            // PLAN A ("iOS / Android") 
            try
            {
                // uh, its no working for UWP mode :(
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;

                    // uh, its no working for UWP mode , too !
                    Location = await GetCity(location);

                    GetWeatherInfo();

                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ex. msg:" + ex.Message);

                
                
            }

            // PLAN B ("UWP")

            Debug.WriteLine("PLAN B!");

            // Dirty Hack: redo it!!!
            //while (geoInfo.GetLatitude() == 0) { };

            // if status "Access to location is denied"...
            if (geoInfo.GetStatus() == 2)
            {
                //TODO
                //await DisplayAlert("Caution", "Access to location is denied. Please unblock it (change Weather app permission)", "OK");
            }

            Latitude = geoInfo.GetLatitude();
            Longitude = geoInfo.GetLongitude();

            // Experimental!            
            Location = await GetCityName(Latitude, Longitude);

            Debug.WriteLine("GetCity: " + Location);

            GetWeatherInfo();

            return;
        }

        // GetCity : Return Current City (Plan A)
        private async Task<string> GetCity(Location location)
        {
            var places = await Geocoding.GetPlacemarksAsync(location);
            var currentPlace = places?.FirstOrDefault();


            if (currentPlace != null)
                 return $"{currentPlace.Locality}{currentPlace.CountryName}";

            return null;
            
        }


        // GetCity : Return Current City (Plan B)
        private async Task<string> GetCityName(double lat, double lon)
        {
            string CityName = "";

            //var url = $"http://api.openweathermap.org/data/2.5/weather?q=" + Location + "&appid="
            //   + APIKey + "&units=metric";

            var url = 
         $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={APIKey}";

            var result = await ApiCaller.Get(url);

            if (result.Successful)
            {
                try
                {
                    //
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);
                    CityName = weatherInfo.name;
                }
                catch
                {
                    Debug.WriteLine("CityName: deserialization error!");
                }
            }
            else
            {
                //
                Debug.WriteLine("CityName: OpenWheather API call error!");
            }

            // Dirtyhack =)
            if (CityName == "Zheleznodorozhnyy")
            {
                CityName = "Moscow"; // Ze. is invisible part of M. ;)
            }
            
            return CityName;
        }//


        public async void GetBackground()
        {
            var url = $"https://api.pexels.com/v1/search?query={Location}&per%20page=15&page=1";

            var result = await
                ApiCaller.Get(
                              url,
                              "563492ad6f917000010000016cbc65bf4f724840ab22e86ed18b2c15"
                              );

            if (result.Successful)
            {
                var bgInfo = JsonConvert.DeserializeObject<BackgroundInfo>(result.Response);

                if (bgInfo != null & bgInfo.photos.Length > 0)
                {
                    Debug.WriteLine("GetBackground - we are here!");
                    Debug.WriteLine(bgInfo.photos.Length);
                    // case 1: get first photo from photo collection
                    //bgImg.Source = ImageSource.FromUri(new Uri(bgInfo.photos[0].src.medium));

                    // case 2: get random photo from photo collection
                    bgImg.Source = ImageSource.FromUri
                        (new Uri
                          (
                            bgInfo.photos[new Random().Next(0, bgInfo.photos.Length - 1)].src.medium
                          )
                        );
                }
            }
            else
            {
                Debug.WriteLine("GetBackground problems: " + result.ErrorMessage);
            }
        }

        private async void GetWeatherInfo()
        {

            var url = $"http://api.openweathermap.org/data/2.5/weather?q=" + Location + "&appid="
                + APIKey + "&units=metric";

            var result = await ApiCaller.Get(url);




            if (result.Successful)
            {
                try
                {

                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);


                    // "Weather Status"
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();

                    //iconImg.Source = $"w{weatherInfo.weather[0].icon}";

                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                            break;
                        case Device.iOS:
                            iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                            break;
                        case Device.UWP:
                            iconImg.Source = $"w{weatherInfo.weather[0].icon}.png";
                            break;
                    }

                    cityTxt.Text = weatherInfo.name.ToUpper();

                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");//("0") - округление до нуля

                    //gpsStatus.Text = geoInfo.GetInfo() + " " +
                    //    geoInfo.GetLatitude() + " " +
                    //    geoInfo.GetLongitude();// Debug Only!

                    if (geoInfo.GetStatus() == 2)
                    {
                        gpsStatus.Text = "Access to location is denied";
                    }
                    else
                        gpsStatus.Text = "";


                    humidityTxt.Text = $"{weatherInfo.main.humidity}%";

                    pressureTxt.Text = $"{weatherInfo.main.pressure} hpa";

                    windTxt.Text = $"{weatherInfo.wind.speed} m/s";

                    cloudinessTxt.Text = $"{weatherInfo.clouds.all}%";

                    lon = weatherInfo.coord.lon.ToString();
                    lat = weatherInfo.coord.lat.ToString();

                    //.ToUniversalTime();
                    var dt = new DateTime(1970, 1, 1).AddSeconds(weatherInfo.dt);

                    //dateTxt.Text = dt.ToString("dddd  MMM, dd yyyy").ToUpper();
                    dateTxt.Text = dt.ToString("dddd  MMM, dd").ToUpper();

                    // DEBUG ONLY: disable to API min. use =) 
                    GetForecast();

                    //DEBUG ONLY
                    //await DisplayAlert("Successful =)", 
                    //    "Debug Info: " + weatherInfo.main.temp, "OK");

                    // Get Pexel's image and set as "app" background =)'
                    GetBackground();
                }
                catch (Exception ex)
                {
                    await DisplayAlert(result.ErrorMessage, "Exception: " + ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "No weather information found", "OK");
                //await DisplayAlert("Weather Info Errors (class1)", 
                //    "Error: " + result.ErrorMessage 
                //    + "  ("+result.Response + ")", "OK");
            }
        }

        private async void GetForecast()
        {
            //var url = 
            //    "http://api.openweathermap.org/data/2.5/forecast?q={Location}&appid="
            //    +APIKey+"&units=metric";

            var url =
                "https://api.openweathermap.org/data/2.5/onecall?lat=" + lat +
                "&lon=" + lon +
                "&exclude=current,minutely,hourly,alerts" +
                "&appid=" + APIKey +
                "&units=metric";

            var result = await ApiCaller.Get(url);

            if (result.Successful)
            {
                try
                {

                    //Debug.WriteLine("result.Response:");
                    //Debug.WriteLine(result.Response);

                    var forcastInfo = JsonConvert.DeserializeObject<OneCallInfo>(result.Response);

                    //Debug.WriteLine("forcastInfo:");
                    //Debug.WriteLine(forcastInfo);

                    List<ForecastList> allList = new List<ForecastList>();

                    foreach (var list in forcastInfo.daily)
                    {

                        //Debug.WriteLine("list:");
                        //Debug.WriteLine(list);

                        var date = new DateTime(1970, 1, 1).AddSeconds(list.dt);
                        var dt_txt = date.ToString("dddd");

                        //var date = DateTime.ParseExact(dt_txt, "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
                        //var date = DateTime.Parse(dt_txt);

                        //Debug.WriteLine(date);
                        //Debug.WriteLine(dt_txt);

                        if (date > DateTime.Now)// && date.Hour == 0 && date.Minute == 0 && date.Second == 0)
                        {
                            allList.Add(list);

                            //Debug.WriteLine("DT_TXT added:");
                            //Debug.WriteLine(dt_txt);
                        }
                    }


                    // +1 day, +2 day, +3 day, +4 day =)
                    for (int i = 0; i < 4; i++)
                    {
                        var dt = new DateTime(1970, 1, 1).AddSeconds(allList[i].dt);
                        dayOneTxt.Text = dt.ToString("dddd");
                        dateOneTxt.Text = dt.ToString("dd MMM");
                        switch (Device.RuntimePlatform)
                        {
                            case Device.Android:
                                iconOneImg.Source = $"w{allList[i].weather[0].icon}";
                                break;
                            case Device.iOS:
                                iconOneImg.Source = $"w{allList[i].weather[0].icon}";
                                break;
                            case Device.UWP:
                                iconOneImg.Source = $"w{allList[i].weather[0].icon}.png";
                                break;
                        }
                        tempOneTxt.Text = allList[i].temp.day.ToString("0");
                    }

                    /*
                    // +1 day
                    var dt = new DateTime(1970, 1, 1).AddSeconds(allList[0].dt);
                    dayOneTxt.Text = dt.ToString("dddd");
                    dateOneTxt.Text = dt.ToString("dd MMM");
                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                            break;
                        case Device.iOS:
                            iconOneImg.Source = $"w{allList[0].weather[0].icon}";
                            break;
                        case Device.UWP:
                            iconOneImg.Source = $"w{allList[0].weather[0].icon}.png";
                            break;
                    }
                    tempOneTxt.Text = allList[0].temp.day.ToString("0");



                    // + 2 day
                    dt = new DateTime(1970, 1, 1).AddSeconds(allList[1].dt);
                    dayTwoTxt.Text = dt.ToString("dddd");
                    dateTwoTxt.Text = dt.ToString("dd MMM");

                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                            break;
                        case Device.iOS:
                            iconTwoImg.Source = $"w{allList[1].weather[0].icon}";
                            break;
                        case Device.UWP:
                            iconTwoImg.Source = $"w{allList[1].weather[0].icon}.png";
                            break;
                    }
                    tempTwoTxt.Text = allList[1].temp.day.ToString("0");


                    // + 3 day
                    dt = new DateTime(1970, 1, 1).AddSeconds(allList[2].dt);
                    dayThreeTxt.Text = dt.ToString("dddd");
                    dateThreeTxt.Text = dt.ToString("dd MMM");

                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                            break;
                        case Device.iOS:
                            iconThreeImg.Source = $"w{allList[2].weather[0].icon}";
                            break;
                        case Device.UWP:
                            iconThreeImg.Source = $"w{allList[2].weather[0].icon}.png";
                            break;
                    }
                    tempThreeTxt.Text = allList[2].temp.day.ToString("0");

                    // + 4 day
                    dt = new DateTime(1970, 1, 1).AddSeconds(allList[3].dt);
                    dayFourTxt.Text = dt.ToString("dddd");
                    dateFourTxt.Text = dt.ToString("dd MMM");
                    switch (Device.RuntimePlatform)
                    {
                        case Device.Android:
                            iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                            break;
                        case Device.iOS:
                            iconFourImg.Source = $"w{allList[3].weather[0].icon}";
                            break;
                        case Device.UWP:
                            iconFourImg.Source = $"w{allList[3].weather[0].icon}.png";
                            break;
                    }
                    tempFourTxt.Text = allList[3].temp.day.ToString("0");
                    */

                    //DEBUG
                    //await DisplayAlert("Weather Info [>>]", lat + 
                    //    " | "+lon + "|" + forcastInfo.daily,
                    //    "OK");


                }
                catch (Exception ex)
                {

                    await DisplayAlert("Exception", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Warning", "Forecast information not available", "OK");
                //await DisplayAlert("Forcast Info Errors (class2)", 
                //    "Error: " + result.ErrorMessage 
                //    + "  ("+result.Response+")" + "!"+lon+"!"+lat, "OK");
            }
        }
    }
}
