using AppCommon.Helpers;
using System.Diagnostics;
using System.Net;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Threading;
using WeatherStation.Api;
using WeatherStation.Infrastructure;
using WeatherStation.RemoteData.GeoApiCommunes;
using WeatherStation.RemoteData.IPGeolocation;
using WeatherStation.RemoteData.NominisCef;
using WeatherStation.WeatherData.InfoClimat;
using WeatherStation.Windows;

namespace WeatherStation
{
    [SupportedOSPlatform("windows")]
    public partial class MainWindow : BaseWindow
    {
        #region Fields
        private const int ForecastUpdateIntervalMinutes = 30;
        private RestApiServer _apiServer = null!;
        private readonly DispatcherTimer _timerForecast;
        private readonly DispatcherTimer _timerClock;
        private City? _currentCity = null;
        private string _currentTimeString = DateTime.Now.ToString("HH:mm");
        private string _sunrise = "Sunrise";
        private string _sunset = "Sunset";
        private string _moonrise = "Moonrise";
        private string _moonset = "Moonset";
        private string _saintOfTheDay = "SaintOfTheDay";
        private HttpStatusCode?[] _apiError;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up the main window components and initializes the forecast and clock timers.
        /// The forecast timer interval is set to the delay until the next forecast update slot,
        /// as defined by <see cref="ForecastUpdateIntervalMinutes"/>. The clock timer interval is set to update every minute.
        /// Event handlers are attached to each timer's Tick event to trigger periodic updates.
        /// </summary>
        public MainWindow()
        {
            _apiError = new HttpStatusCode?[3] { HttpStatusCode.NotFound, HttpStatusCode.NotFound, HttpStatusCode.NotFound };
            InitializeComponent();
            // Create and configure the forecast timer with the interval until the next forecast update slot
            _timerForecast = new DispatcherTimer
            {
                Interval = GetDelayToNextSlot(ForecastUpdateIntervalMinutes)
            };
            _timerForecast.Tick += TimerForecast_Tick;
            // Create and configure the clock timer with the interval until the next minute
            _timerClock = new DispatcherTimer
            {
                Interval = GetDelayToNextSlot(1)
            };
            _timerClock.Tick += TimerClock_Tick;

        }

        #endregion

        protected override void FirstInit()
        {
            base.FirstInit();
            _apiServer = new RestApiServer();
            _apiServer.SensorDataReceived += (sender, data) =>
            {
                Dispatcher.Invoke(() =>
                {
                    SensorDisplayMain.SensorDataList.UpdateSensor(data);
                });
            };
            _currentCity = AppParameters.Settings.CurrentCity ?? City.GetDefaultCity();
            TemperatureBarCurrentDay.MinTemp = -5;
            TemperatureBarCurrentDay.MaxTemp = 30;
        }

        protected override void DelayedFirstInit()
        {
            base.DelayedFirstInit();
            SearchNetworkInterfaces();
            _timerForecast.Start();
            _timerClock.Start();
            _ = UpdateData();
            SetComponents();
            TemperatureBarCurrentDay.Values = [-5, 30, -5, 30, -5, 30, -5, 30];
        }


        protected override void SetComponents()
        {
            base.SetComponents();
            TextBlockCurrentTime.Text = _currentTimeString;
            TextBlockCurrentCity.Text = !string.IsNullOrWhiteSpace(_currentCity?.FormattedName) ? _currentCity?.FormattedName : "No city selected";
            TextBlockCurrentCoordinates.Text = _currentCity is object ? $"{_currentCity.Center.coordinates[1].ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}, {_currentCity.Center.coordinates[0].ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}" : string.Empty;
            TextBlockSunrise.Text = _sunrise;
            TextBlockSunset.Text = _sunset;
            TextBlockMoonset.Text = _moonset;
            TextBlockMoonrise.Text = _moonrise;
            TextBlockSaintOfTheDay.Text = _saintOfTheDay;
            ImageApiWarning.Visibility = _apiError.Any(x => x is not null) ? Visibility.Visible : Visibility.Collapsed;
        }



        private async Task UpdateData()
        {
            await UpdateForecast(_currentCity);
            await UpdateEphemeris(_currentCity);
            await UpdateNominis();
            SetComponents();
        }

        /// <summary>
        /// Calculates the delay until the next time slot, based on the specified interval in minutes.
        /// For example, with intervalMinutes = 30, the next slot will be on the hour or half hour.
        /// </summary>
        /// <param name="intervalMinutes">The interval in minutes between each slot (e.g., 30 for half-hour, 15 for quarter-hour).</param>
        /// <returns>The TimeSpan until the next slot.</returns>
        private static TimeSpan GetDelayToNextSlot(int intervalMinutes)
        {
            var now = DateTime.Now;
            var minutesPastSlot = now.Minute % intervalMinutes;
            var minutesToNextSlot = intervalMinutes - minutesPastSlot;
            var nextSlot = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                now.Hour,
                now.Minute,
                0,
                DateTimeKind.Local).AddMinutes(minutesToNextSlot);
            return nextSlot - now;
        }

        /// <summary>
        /// Searches for available network interfaces and selects one to use for the application.
        /// </summary>
        private async void SearchNetworkInterfaces()
        {
            IPAddress? _localIp = null;
            var networkInterfaces = NetworkHelper.GetPotentialNetworkInterfaces();
            if (networkInterfaces != null)
            {
                switch (networkInterfaces.Count)
                {
                    case 0:
                        LabelLocalIp.Content = "No network interface available";
                        break;
                    case 2:
                        AppSettingsManager.Instance.Settings.LastUsedNetworkInterfaceId = networkInterfaces[0].Id;
                        _localIp = networkInterfaces[0].IPAddressV4;
                        break;
                    default:
                        _localIp = SelectNetworkInterface(networkInterfaces);
                        break;
                }
                if (_localIp is object)
                {
                    await _apiServer.StartAsync(_localIp);
                    LabelLocalIp.Content = $"Local IP: {_localIp}";
                }
                else
                {
                    LabelLocalIp.Content = "No selected network interface";
                }
            }
        }

        /// <summary>
        /// Selects a network interface from the provided list of network interfaces.
        /// </summary>
        /// <param name="networkInterfaces">
        /// A list of <see cref="NetworkHelper.NetworkInterfaceInfo"/> objects representing the available network interfaces.
        /// </param>
        /// <returns>
        /// The <see cref="IPAddress"/> of the selected network interface, or <c>null</c> if no interface is selected.
        /// </returns>
        private IPAddress? SelectNetworkInterface(List<NetworkHelper.NetworkInterfaceInfo> networkInterfaces)
        {
            // Check if the last used network interface is still available            
            var lastUsedInterface = networkInterfaces.FirstOrDefault(ni => ni.Id == AppSettingsManager.Instance.Settings.LastUsedNetworkInterfaceId);
            if (lastUsedInterface != null)
            {
                return lastUsedInterface.IPAddressV4;
            }
            var selectNetworkInterfaceWindow = new SelectNetworkInterfaceWindow(this, networkInterfaces);
            if (selectNetworkInterfaceWindow.ShowDialog() == true)
            {
                AppSettingsManager.Instance.Settings.LastUsedNetworkInterfaceId = selectNetworkInterfaceWindow.SelectedNetworkInterfaceInfo!.Id;
                return selectNetworkInterfaceWindow.SelectedNetworkInterfaceInfo?.IPAddressV4;
            }
            return null;
        }

        /// <summary>
        /// Asynchronously updates the weather forecast displayed in the main window.
        /// This method creates a new instance of <see cref="InfoClimatManager"/>, loads the latest weather data,
        /// and updates the current and future weather cards with this information.
        /// The current weather card is updated for the present day, while each of the four future weather cards
        /// is updated for the corresponding day (from day 1 to day 4 ahead).
        /// </summary>
        /// <remarks>
        /// This method is intended to be called periodically (e.g., by a timer) or on demand to refresh the weather data
        /// shown to the user. It ensures that all weather cards reflect the most recent forecast information.
        /// </remarks>
        private async Task UpdateForecast(City? city)
        {
            if (city is null) { return; }
            var dayForecastCount = 0;
            var infoClimatManager = new InfoClimatManager(city);
            await infoClimatManager.LoadInfoClimatDataAsync(_apiError);
            CurrentWeatherCard.ForecastDate = DateOnly.FromDateTime(DateTime.Now);
            CurrentWeatherCard.UpdateCard(infoClimatManager);
            var dayForecasts = infoClimatManager.GetForecastsForDay(DateOnly.FromDateTime(DateTime.Now));
            for (var i = 1; i <= 4; i++)
            {
                switch (i)
                {
                    case 1:
                        FutureWeatherCardDay1.ForecastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                        FutureWeatherCardDay1.UpdateCard(infoClimatManager);
                        dayForecastCount = Math.Max(dayForecastCount, infoClimatManager.GetCountForecastForDay(DateOnly.FromDateTime(DateTime.Now.AddDays(i))));
                        break;
                    case 2:
                        FutureWeatherCardDay2.ForecastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                        FutureWeatherCardDay2.UpdateCard(infoClimatManager);
                        dayForecastCount = Math.Max(dayForecastCount, infoClimatManager.GetCountForecastForDay(DateOnly.FromDateTime(DateTime.Now.AddDays(i))));
                        break;
                    case 3:
                        FutureWeatherCardDay3.ForecastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                        FutureWeatherCardDay3.UpdateCard(infoClimatManager);
                        dayForecastCount = Math.Max(dayForecastCount, infoClimatManager.GetCountForecastForDay(DateOnly.FromDateTime(DateTime.Now.AddDays(i))));
                        break;
                    case 4:
                        FutureWeatherCardDay4.ForecastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                        FutureWeatherCardDay4.UpdateCard(infoClimatManager);
                        dayForecastCount = Math.Max(dayForecastCount, infoClimatManager.GetCountForecastForDay(DateOnly.FromDateTime(DateTime.Now.AddDays(i))));
                        break;
                }
            }

            while (dayForecasts?.Count < dayForecastCount)
            {
                dayForecasts.Insert(0, null);
            }
            TemperatureBarCurrentDay.ForecastDatas = dayForecasts;
        }


        private async Task UpdateEphemeris(City? city)
        {
            if (city is null) { return; }
            var ephemeris = new Ephemeris(city);
            var ephemerisData = await ephemeris.LoadEphemerisDataAsync(_apiError);
            _sunrise = ephemerisData?.Sunrise ?? "Sunrise";
            _sunset = ephemerisData?.Sunset ?? "Sunset";
            _moonrise = ephemerisData?.Moonset ?? "Moonrise";
            _moonset = ephemerisData?.Moonrise ?? "Moonset";
        }

        private async Task UpdateNominis()
        {
            var nominis = new Nominis();
            var nominisData = await Nominis.LoadNominisDataAsync(_apiError);
            _saintOfTheDay = nominisData?.Response?.SaintOfTheDay?.Name ?? "SaintOfTheDay";
        }

        /// <summary>
        /// Opens the tools window, allowing the user to modify or select additional tools or settings related to the current _city.
        /// If the user confirms their selection in the tools window, the current _city is updated accordingly,
        /// the application settings are synchronized, the UI components are refreshed, and the weather forecast is updated for the new _city.
        /// </summary>
        private void ShowTools()
        {
            var toolsWindow = new ToolsWindow(this, _currentCity);
            if (toolsWindow.ShowDialog() == true)
            {
                _currentCity = toolsWindow.CurrentCity;
                AppParameters.Settings.CurrentCity = _currentCity;
                _ = UpdateData();
                SetComponents();
            }
        }

        #region example data
        private string GetWheatherJson()
        {
            var str = """
                                {
                    "request_state": 200,
                    "request_key": "fd543c77e33d6c8a5e218e948a19e487",
                    "message": "OK",
                    "model_run": "08",
                    "source": "internal:GFS:1",
                    "2025-04-23 11:00:00": {
                        "temperature": {
                            "2m": 283.8,
                            "sol": 283.1,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102220
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 70.9
                        },
                        "vent_moyen": {
                            "10m": 12.8
                        },
                        "vent_rafales": {
                            "10m": 40.8
                        },
                        "vent_direction": {
                            "10m": 222
                        },
                        "iso_zero": 2498,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-23 14:00:00": {
                        "temperature": {
                            "2m": 286.4,
                            "sol": 285.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102140
                        },
                        "pluie": 0,
                        "pluie_convective": -0.5,
                        "humidite": {
                            "2m": 77.8
                        },
                        "vent_moyen": {
                            "10m": 18.2
                        },
                        "vent_rafales": {
                            "10m": 45
                        },
                        "vent_direction": {
                            "10m": 249
                        },
                        "iso_zero": 2630,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-23 17:00:00": {
                        "temperature": {
                            "2m": 286.7,
                            "sol": 286.1,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102090
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 93.9
                        },
                        "vent_moyen": {
                            "10m": 17.1
                        },
                        "vent_rafales": {
                            "10m": 42.6
                        },
                        "vent_direction": {
                            "10m": 265
                        },
                        "iso_zero": 2473,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-23 20:00:00": {
                        "temperature": {
                            "2m": 287.3,
                            "sol": 286.8,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102130
                        },
                        "pluie": 0.3,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 84.7
                        },
                        "vent_moyen": {
                            "10m": 16.9
                        },
                        "vent_rafales": {
                            "10m": 36.1
                        },
                        "vent_direction": {
                            "10m": 273
                        },
                        "iso_zero": 2608,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 98
                        }
                    },
                    "2025-04-23 23:00:00": {
                        "temperature": {
                            "2m": 286.1,
                            "sol": 285.8,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102330
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 88.3
                        },
                        "vent_moyen": {
                            "10m": 11.6
                        },
                        "vent_rafales": {
                            "10m": 33.9
                        },
                        "vent_direction": {
                            "10m": 256
                        },
                        "iso_zero": 2878,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 93
                        }
                    },
                    "2025-04-24 02:00:00": {
                        "temperature": {
                            "2m": 285.7,
                            "sol": 285.1,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102330
                        },
                        "pluie": 0.4,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 94.5
                        },
                        "vent_moyen": {
                            "10m": 10.6
                        },
                        "vent_rafales": {
                            "10m": 30.3
                        },
                        "vent_direction": {
                            "10m": 256
                        },
                        "iso_zero": 2801,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 97
                        }
                    },
                    "2025-04-24 05:00:00": {
                        "temperature": {
                            "2m": 285.6,
                            "sol": 285,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102240
                        },
                        "pluie": 0.5,
                        "pluie_convective": 0.4,
                        "humidite": {
                            "2m": 96.7
                        },
                        "vent_moyen": {
                            "10m": 10.3
                        },
                        "vent_rafales": {
                            "10m": 28.8
                        },
                        "vent_direction": {
                            "10m": 247
                        },
                        "iso_zero": 3148,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-24 08:00:00": {
                        "temperature": {
                            "2m": 285.6,
                            "sol": 285,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102250
                        },
                        "pluie": 0.3,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 96.4
                        },
                        "vent_moyen": {
                            "10m": 10.9
                        },
                        "vent_rafales": {
                            "10m": 30.3
                        },
                        "vent_direction": {
                            "10m": 249
                        },
                        "iso_zero": 3197,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-24 11:00:00": {
                        "temperature": {
                            "2m": 287.2,
                            "sol": 286.2,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102300
                        },
                        "pluie": 0.4,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 90.6
                        },
                        "vent_moyen": {
                            "10m": 8.6
                        },
                        "vent_rafales": {
                            "10m": 23.1
                        },
                        "vent_direction": {
                            "10m": 275
                        },
                        "iso_zero": 3298,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-24 14:00:00": {
                        "temperature": {
                            "2m": 291.1,
                            "sol": 289.5,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102160
                        },
                        "pluie": 0.3,
                        "pluie_convective": 0.4,
                        "humidite": {
                            "2m": 59.6
                        },
                        "vent_moyen": {
                            "10m": 10.7
                        },
                        "vent_rafales": {
                            "10m": 14.4
                        },
                        "vent_direction": {
                            "10m": 292
                        },
                        "iso_zero": 3353,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 98
                        }
                    },
                    "2025-04-24 17:00:00": {
                        "temperature": {
                            "2m": 293.7,
                            "sol": 291.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102010
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 46.3
                        },
                        "vent_moyen": {
                            "10m": 7.6
                        },
                        "vent_rafales": {
                            "10m": 12.3
                        },
                        "vent_direction": {
                            "10m": 301
                        },
                        "iso_zero": 3422,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 78
                        }
                    },
                    "2025-04-24 20:00:00": {
                        "temperature": {
                            "2m": 290.4,
                            "sol": 290.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101970
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 60.9
                        },
                        "vent_moyen": {
                            "10m": 10.3
                        },
                        "vent_rafales": {
                            "10m": 20.9
                        },
                        "vent_direction": {
                            "10m": 325
                        },
                        "iso_zero": 3438,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 48
                        }
                    },
                    "2025-04-24 23:00:00": {
                        "temperature": {
                            "2m": 285,
                            "sol": 286.2,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102090
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 84.2
                        },
                        "vent_moyen": {
                            "10m": 6.2
                        },
                        "vent_rafales": {
                            "10m": 16.3
                        },
                        "vent_direction": {
                            "10m": 367
                        },
                        "iso_zero": 3455,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 0
                        }
                    },
                    "2025-04-25 02:00:00": {
                        "temperature": {
                            "2m": 283.4,
                            "sol": 284.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101990
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 92.5
                        },
                        "vent_moyen": {
                            "10m": 6
                        },
                        "vent_rafales": {
                            "10m": 11.7
                        },
                        "vent_direction": {
                            "10m": 398
                        },
                        "iso_zero": 3487,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 2
                        }
                    },
                    "2025-04-25 05:00:00": {
                        "temperature": {
                            "2m": 282.4,
                            "sol": 283.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101870
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 95.4
                        },
                        "vent_moyen": {
                            "10m": 6.1
                        },
                        "vent_rafales": {
                            "10m": 8.7
                        },
                        "vent_direction": {
                            "10m": 422
                        },
                        "iso_zero": 3407,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 0
                        }
                    },
                    "2025-04-25 08:00:00": {
                        "temperature": {
                            "2m": 282.9,
                            "sol": 282.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101840
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 92.4
                        },
                        "vent_moyen": {
                            "10m": 4.8
                        },
                        "vent_rafales": {
                            "10m": 12.3
                        },
                        "vent_direction": {
                            "10m": 439
                        },
                        "iso_zero": 3365,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 0
                        }
                    },
                    "2025-04-25 11:00:00": {
                        "temperature": {
                            "2m": 290.2,
                            "sol": 288.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101770
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 56.9
                        },
                        "vent_moyen": {
                            "10m": 7.3
                        },
                        "vent_rafales": {
                            "10m": 10.2
                        },
                        "vent_direction": {
                            "10m": 429
                        },
                        "iso_zero": 3316,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 32
                        }
                    },
                    "2025-04-25 14:00:00": {
                        "temperature": {
                            "2m": 295.4,
                            "sol": 293,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101570
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 38
                        },
                        "vent_moyen": {
                            "10m": 7.1
                        },
                        "vent_rafales": {
                            "10m": 8
                        },
                        "vent_direction": {
                            "10m": 418
                        },
                        "iso_zero": 3308,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 21
                        }
                    },
                    "2025-04-25 17:00:00": {
                        "temperature": {
                            "2m": 296.7,
                            "sol": 295.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101420
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 33.1
                        },
                        "vent_moyen": {
                            "10m": 7.7
                        },
                        "vent_rafales": {
                            "10m": 6.3
                        },
                        "vent_direction": {
                            "10m": 393
                        },
                        "iso_zero": 3260,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 6
                        }
                    },
                    "2025-04-25 20:00:00": {
                        "temperature": {
                            "2m": 292.3,
                            "sol": 293,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101410
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 53.6
                        },
                        "vent_moyen": {
                            "10m": 6.3
                        },
                        "vent_rafales": {
                            "10m": 10.9
                        },
                        "vent_direction": {
                            "10m": 403
                        },
                        "iso_zero": 3193,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 39
                        }
                    },
                    "2025-04-25 23:00:00": {
                        "temperature": {
                            "2m": 287.9,
                            "sol": 289.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101650
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 69.5
                        },
                        "vent_moyen": {
                            "10m": 1.3
                        },
                        "vent_rafales": {
                            "10m": 1.5
                        },
                        "vent_direction": {
                            "10m": 233
                        },
                        "iso_zero": 3063,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 74
                        }
                    },
                    "2025-04-26 02:00:00": {
                        "temperature": {
                            "2m": 286.9,
                            "sol": 286.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101730
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 82.1
                        },
                        "vent_moyen": {
                            "10m": 9.7
                        },
                        "vent_rafales": {
                            "10m": 25.7
                        },
                        "vent_direction": {
                            "10m": 285
                        },
                        "iso_zero": 2805,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 87
                        }
                    },
                    "2025-04-26 05:00:00": {
                        "temperature": {
                            "2m": 286.4,
                            "sol": 286,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101740
                        },
                        "pluie": 0.4,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 91.9
                        },
                        "vent_moyen": {
                            "10m": 12.9
                        },
                        "vent_rafales": {
                            "10m": 33.6
                        },
                        "vent_direction": {
                            "10m": 308
                        },
                        "iso_zero": 2335,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-26 08:00:00": {
                        "temperature": {
                            "2m": 285.2,
                            "sol": 284.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101900
                        },
                        "pluie": 2.4,
                        "pluie_convective": 0.2,
                        "humidite": {
                            "2m": 96.3
                        },
                        "vent_moyen": {
                            "10m": 7.6
                        },
                        "vent_rafales": {
                            "10m": 25.9
                        },
                        "vent_direction": {
                            "10m": 268
                        },
                        "iso_zero": 2130,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-26 11:00:00": {
                        "temperature": {
                            "2m": 285.1,
                            "sol": 284.5,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102030
                        },
                        "pluie": 1.9,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 95
                        },
                        "vent_moyen": {
                            "10m": 10.7
                        },
                        "vent_rafales": {
                            "10m": 28.6
                        },
                        "vent_direction": {
                            "10m": 290
                        },
                        "iso_zero": 2158,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-26 14:00:00": {
                        "temperature": {
                            "2m": 291,
                            "sol": 288.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101990
                        },
                        "pluie": 0.7,
                        "pluie_convective": 0.7,
                        "humidite": {
                            "2m": 58.3
                        },
                        "vent_moyen": {
                            "10m": 14.9
                        },
                        "vent_rafales": {
                            "10m": 23.6
                        },
                        "vent_direction": {
                            "10m": 299
                        },
                        "iso_zero": 2198,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 82
                        }
                    },
                    "2025-04-26 17:00:00": {
                        "temperature": {
                            "2m": 290.7,
                            "sol": 289.2,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102010
                        },
                        "pluie": 1.4,
                        "pluie_convective": 1.4,
                        "humidite": {
                            "2m": 58.9
                        },
                        "vent_moyen": {
                            "10m": 18.7
                        },
                        "vent_rafales": {
                            "10m": 28.1
                        },
                        "vent_direction": {
                            "10m": 302
                        },
                        "iso_zero": 2198,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 76
                        }
                    },
                    "2025-04-26 20:00:00": {
                        "temperature": {
                            "2m": 287.5,
                            "sol": 286.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102190
                        },
                        "pluie": 1.1,
                        "pluie_convective": 1.1,
                        "humidite": {
                            "2m": 76.4
                        },
                        "vent_moyen": {
                            "10m": 14.2
                        },
                        "vent_rafales": {
                            "10m": 28
                        },
                        "vent_direction": {
                            "10m": 306
                        },
                        "iso_zero": 2141,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 49
                        }
                    },
                    "2025-04-26 23:00:00": {
                        "temperature": {
                            "2m": 284.3,
                            "sol": 284.1,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102430
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 92.6
                        },
                        "vent_moyen": {
                            "10m": 10.2
                        },
                        "vent_rafales": {
                            "10m": 29.4
                        },
                        "vent_direction": {
                            "10m": 282
                        },
                        "iso_zero": 2039,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 28
                        }
                    },
                    "2025-04-27 02:00:00": {
                        "temperature": {
                            "2m": 284,
                            "sol": 283.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102370
                        },
                        "pluie": 0.4,
                        "pluie_convective": 0.4,
                        "humidite": {
                            "2m": 97.1
                        },
                        "vent_moyen": {
                            "10m": 6.4
                        },
                        "vent_rafales": {
                            "10m": 20
                        },
                        "vent_direction": {
                            "10m": 249
                        },
                        "iso_zero": 2029,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 49
                        }
                    },
                    "2025-04-27 05:00:00": {
                        "temperature": {
                            "2m": 284.1,
                            "sol": 283.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102330
                        },
                        "pluie": 0.3,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 98.1
                        },
                        "vent_moyen": {
                            "10m": 4.1
                        },
                        "vent_rafales": {
                            "10m": 8.3
                        },
                        "vent_direction": {
                            "10m": 253
                        },
                        "iso_zero": 1933,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-27 08:00:00": {
                        "temperature": {
                            "2m": 284.5,
                            "sol": 283.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102400
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 95.4
                        },
                        "vent_moyen": {
                            "10m": 3.4
                        },
                        "vent_rafales": {
                            "10m": 6.5
                        },
                        "vent_direction": {
                            "10m": 261
                        },
                        "iso_zero": 2043,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-27 11:00:00": {
                        "temperature": {
                            "2m": 289,
                            "sol": 286.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102420
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 69.1
                        },
                        "vent_moyen": {
                            "10m": 5.9
                        },
                        "vent_rafales": {
                            "10m": 8.7
                        },
                        "vent_direction": {
                            "10m": 318
                        },
                        "iso_zero": 2047,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 76
                        }
                    },
                    "2025-04-27 14:00:00": {
                        "temperature": {
                            "2m": 293.7,
                            "sol": 290.2,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102330
                        },
                        "pluie": 0.4,
                        "pluie_convective": 0.4,
                        "humidite": {
                            "2m": 45.9
                        },
                        "vent_moyen": {
                            "10m": 4.3
                        },
                        "vent_rafales": {
                            "10m": 9.8
                        },
                        "vent_direction": {
                            "10m": 381
                        },
                        "iso_zero": 2099,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 68
                        }
                    },
                    "2025-04-27 17:00:00": {
                        "temperature": {
                            "2m": 291.4,
                            "sol": 290.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102240
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 56.1
                        },
                        "vent_moyen": {
                            "10m": 4.2
                        },
                        "vent_rafales": {
                            "10m": 10
                        },
                        "vent_direction": {
                            "10m": 344
                        },
                        "iso_zero": 2207,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 87
                        }
                    },
                    "2025-04-27 20:00:00": {
                        "temperature": {
                            "2m": 289.4,
                            "sol": 288.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102270
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 68.5
                        },
                        "vent_moyen": {
                            "10m": 8.3
                        },
                        "vent_rafales": {
                            "10m": 18.6
                        },
                        "vent_direction": {
                            "10m": 317
                        },
                        "iso_zero": 2169,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 93
                        }
                    },
                    "2025-04-27 23:00:00": {
                        "temperature": {
                            "2m": 287.7,
                            "sol": 288.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102380
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 74.7
                        },
                        "vent_moyen": {
                            "10m": 6.8
                        },
                        "vent_rafales": {
                            "10m": 25.6
                        },
                        "vent_direction": {
                            "10m": 358
                        },
                        "iso_zero": 3090,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 99
                        }
                    },
                    "2025-04-28 02:00:00": {
                        "temperature": {
                            "2m": 284.1,
                            "sol": 285.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102340
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 86.1
                        },
                        "vent_moyen": {
                            "10m": 1.7
                        },
                        "vent_rafales": {
                            "10m": 2.2
                        },
                        "vent_direction": {
                            "10m": 518
                        },
                        "iso_zero": 3402,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 49
                        }
                    },
                    "2025-04-28 05:00:00": {
                        "temperature": {
                            "2m": 282.8,
                            "sol": 284.5,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102200
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 88.3
                        },
                        "vent_moyen": {
                            "10m": 5.3
                        },
                        "vent_rafales": {
                            "10m": 5.5
                        },
                        "vent_direction": {
                            "10m": 446
                        },
                        "iso_zero": 3570,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 1
                        }
                    },
                    "2025-04-28 08:00:00": {
                        "temperature": {
                            "2m": 283.2,
                            "sol": 283.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 102100
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 82.5
                        },
                        "vent_moyen": {
                            "10m": 6
                        },
                        "vent_rafales": {
                            "10m": 17
                        },
                        "vent_direction": {
                            "10m": 456
                        },
                        "iso_zero": 3578,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 4
                        }
                    },
                    "2025-04-28 11:00:00": {
                        "temperature": {
                            "2m": 291.2,
                            "sol": 289.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101910
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 47.9
                        },
                        "vent_moyen": {
                            "10m": 10.7
                        },
                        "vent_rafales": {
                            "10m": 16.6
                        },
                        "vent_direction": {
                            "10m": 456
                        },
                        "iso_zero": 3611,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 8
                        }
                    },
                    "2025-04-28 14:00:00": {
                        "temperature": {
                            "2m": 295.5,
                            "sol": 293.2,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101710
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 34
                        },
                        "vent_moyen": {
                            "10m": 10.1
                        },
                        "vent_rafales": {
                            "10m": 11.6
                        },
                        "vent_direction": {
                            "10m": 469
                        },
                        "iso_zero": 3669,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 21
                        }
                    },
                    "2025-04-28 17:00:00": {
                        "temperature": {
                            "2m": 296.3,
                            "sol": 294.8,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101510
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 34.2
                        },
                        "vent_moyen": {
                            "10m": 12.1
                        },
                        "vent_rafales": {
                            "10m": 12.2
                        },
                        "vent_direction": {
                            "10m": 477
                        },
                        "iso_zero": 3676,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 16
                        }
                    },
                    "2025-04-28 20:00:00": {
                        "temperature": {
                            "2m": 292.3,
                            "sol": 292.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101400
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 54.7
                        },
                        "vent_moyen": {
                            "10m": 9.8
                        },
                        "vent_rafales": {
                            "10m": 23.1
                        },
                        "vent_direction": {
                            "10m": 467
                        },
                        "iso_zero": 3689,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 46
                        }
                    },
                    "2025-04-28 23:00:00": {
                        "temperature": {
                            "2m": 287.7,
                            "sol": 288.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101340
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 69.8
                        },
                        "vent_moyen": {
                            "10m": 9.5
                        },
                        "vent_rafales": {
                            "10m": 33.9
                        },
                        "vent_direction": {
                            "10m": 461
                        },
                        "iso_zero": 3734,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-29 02:00:00": {
                        "temperature": {
                            "2m": 286.5,
                            "sol": 286.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101180
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 67.1
                        },
                        "vent_moyen": {
                            "10m": 10.6
                        },
                        "vent_rafales": {
                            "10m": 37.5
                        },
                        "vent_direction": {
                            "10m": 461
                        },
                        "iso_zero": 3747,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-29 05:00:00": {
                        "temperature": {
                            "2m": 285.4,
                            "sol": 285.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101020
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 69.2
                        },
                        "vent_moyen": {
                            "10m": 12
                        },
                        "vent_rafales": {
                            "10m": 38.9
                        },
                        "vent_direction": {
                            "10m": 463
                        },
                        "iso_zero": 3711,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-29 08:00:00": {
                        "temperature": {
                            "2m": 284.6,
                            "sol": 284.8,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100930
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 73.6
                        },
                        "vent_moyen": {
                            "10m": 11.1
                        },
                        "vent_rafales": {
                            "10m": 32
                        },
                        "vent_direction": {
                            "10m": 465
                        },
                        "iso_zero": 3561,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 96
                        }
                    },
                    "2025-04-29 11:00:00": {
                        "temperature": {
                            "2m": 292.2,
                            "sol": 291.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100810
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 52.3
                        },
                        "vent_moyen": {
                            "10m": 13
                        },
                        "vent_rafales": {
                            "10m": 19.5
                        },
                        "vent_direction": {
                            "10m": 469
                        },
                        "iso_zero": 3433,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 49
                        }
                    },
                    "2025-04-29 14:00:00": {
                        "temperature": {
                            "2m": 297.4,
                            "sol": 296.5,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100690
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 40.6
                        },
                        "vent_moyen": {
                            "10m": 10.3
                        },
                        "vent_rafales": {
                            "10m": 10.8
                        },
                        "vent_direction": {
                            "10m": 473
                        },
                        "iso_zero": 3294,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 71
                        }
                    },
                    "2025-04-29 17:00:00": {
                        "temperature": {
                            "2m": 299,
                            "sol": 298.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100530
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 38.4
                        },
                        "vent_moyen": {
                            "10m": 10.7
                        },
                        "vent_rafales": {
                            "10m": 8.7
                        },
                        "vent_direction": {
                            "10m": 466
                        },
                        "iso_zero": 3248,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 7
                        }
                    },
                    "2025-04-29 20:00:00": {
                        "temperature": {
                            "2m": 294.1,
                            "sol": 294.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100500
                        },
                        "pluie": 0,
                        "pluie_convective": 0,
                        "humidite": {
                            "2m": 57.5
                        },
                        "vent_moyen": {
                            "10m": 13.7
                        },
                        "vent_rafales": {
                            "10m": 29.3
                        },
                        "vent_direction": {
                            "10m": 460
                        },
                        "iso_zero": 3140,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 8
                        }
                    },
                    "2025-04-29 23:00:00": {
                        "temperature": {
                            "2m": 288.5,
                            "sol": 288.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100870
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 82.4
                        },
                        "vent_moyen": {
                            "10m": 10.1
                        },
                        "vent_rafales": {
                            "10m": 18.4
                        },
                        "vent_direction": {
                            "10m": 294
                        },
                        "iso_zero": 3042,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 41
                        }
                    },
                    "2025-04-30 02:00:00": {
                        "temperature": {
                            "2m": 286.4,
                            "sol": 286.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101000
                        },
                        "pluie": 0.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 84.4
                        },
                        "vent_moyen": {
                            "10m": 12.2
                        },
                        "vent_rafales": {
                            "10m": 24.8
                        },
                        "vent_direction": {
                            "10m": 307
                        },
                        "iso_zero": 2991,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 71
                        }
                    },
                    "2025-04-30 05:00:00": {
                        "temperature": {
                            "2m": 285.8,
                            "sol": 285.7,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101060
                        },
                        "pluie": 0.3,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 90.8
                        },
                        "vent_moyen": {
                            "10m": 7.4
                        },
                        "vent_rafales": {
                            "10m": 18.1
                        },
                        "vent_direction": {
                            "10m": 299
                        },
                        "iso_zero": 2925,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-30 08:00:00": {
                        "temperature": {
                            "2m": 284.4,
                            "sol": 284.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101090
                        },
                        "pluie": 1.2,
                        "pluie_convective": 0.2,
                        "humidite": {
                            "2m": 93.4
                        },
                        "vent_moyen": {
                            "10m": 11.4
                        },
                        "vent_rafales": {
                            "10m": 27.9
                        },
                        "vent_direction": {
                            "10m": 323
                        },
                        "iso_zero": 2682,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-30 11:00:00": {
                        "temperature": {
                            "2m": 284.4,
                            "sol": 284.3,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101130
                        },
                        "pluie": 1.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 93.9
                        },
                        "vent_moyen": {
                            "10m": 6.2
                        },
                        "vent_rafales": {
                            "10m": 18.7
                        },
                        "vent_direction": {
                            "10m": 308
                        },
                        "iso_zero": 2116,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-30 14:00:00": {
                        "temperature": {
                            "2m": 284.5,
                            "sol": 284.4,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 101050
                        },
                        "pluie": 3.9,
                        "pluie_convective": 0.4,
                        "humidite": {
                            "2m": 95.5
                        },
                        "vent_moyen": {
                            "10m": 8.8
                        },
                        "vent_rafales": {
                            "10m": 20.7
                        },
                        "vent_direction": {
                            "10m": 301
                        },
                        "iso_zero": 1932,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-30 17:00:00": {
                        "temperature": {
                            "2m": 283.6,
                            "sol": 283.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100980
                        },
                        "pluie": 4.8,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 95.7
                        },
                        "vent_moyen": {
                            "10m": 10
                        },
                        "vent_rafales": {
                            "10m": 22.1
                        },
                        "vent_direction": {
                            "10m": 288
                        },
                        "iso_zero": 1923,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-30 20:00:00": {
                        "temperature": {
                            "2m": 283.8,
                            "sol": 284,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100880
                        },
                        "pluie": 1.1,
                        "pluie_convective": 0.1,
                        "humidite": {
                            "2m": 94.3
                        },
                        "vent_moyen": {
                            "10m": 9.4
                        },
                        "vent_rafales": {
                            "10m": 26.7
                        },
                        "vent_direction": {
                            "10m": 294
                        },
                        "iso_zero": 1887,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-04-30 23:00:00": {
                        "temperature": {
                            "2m": 283.4,
                            "sol": 283.5,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100930
                        },
                        "pluie": 1.9,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 96.5
                        },
                        "vent_moyen": {
                            "10m": 7.6
                        },
                        "vent_rafales": {
                            "10m": 25.2
                        },
                        "vent_direction": {
                            "10m": 302
                        },
                        "iso_zero": 1771,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-05-01 02:00:00": {
                        "temperature": {
                            "2m": 283.3,
                            "sol": 283.6,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100750
                        },
                        "pluie": 0.8,
                        "pluie_convective": 0.2,
                        "humidite": {
                            "2m": 96
                        },
                        "vent_moyen": {
                            "10m": 9.8
                        },
                        "vent_rafales": {
                            "10m": 29.9
                        },
                        "vent_direction": {
                            "10m": 286
                        },
                        "iso_zero": 1782,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-05-01 05:00:00": {
                        "temperature": {
                            "2m": 283.5,
                            "sol": 283.9,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100580
                        },
                        "pluie": 2.8,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 95.3
                        },
                        "vent_moyen": {
                            "10m": 12.4
                        },
                        "vent_rafales": {
                            "10m": 33.9
                        },
                        "vent_direction": {
                            "10m": 293
                        },
                        "iso_zero": 1885,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    },
                    "2025-05-01 08:00:00": {
                        "temperature": {
                            "2m": 283.6,
                            "sol": 284,
                            "500hPa": 273.2,
                            "850hPa": 273.2
                        },
                        "pression": {
                            "niveau_de_la_mer": 100530
                        },
                        "pluie": 5.7,
                        "pluie_convective": 0.3,
                        "humidite": {
                            "2m": 96.7
                        },
                        "vent_moyen": {
                            "10m": 13.4
                        },
                        "vent_rafales": {
                            "10m": 34.9
                        },
                        "vent_direction": {
                            "10m": 302
                        },
                        "iso_zero": 1987,
                        "risque_neige": "non",
                        "cape": 0,
                        "nebulosite": {
                            "haute": 0,
                            "moyenne": 0,
                            "basse": 0,
                            "totale": 100
                        }
                    }
                }

                """;
            return str;
        }

        #endregion

        #region IHM
        private void TimerForecast_Tick(object? sender, EventArgs e)
        {
            _timerForecast.Interval = GetDelayToNextSlot(ForecastUpdateIntervalMinutes);
            _ = UpdateData();
        }

        private void TimerClock_Tick(object? sender, EventArgs e)
        {
            _timerClock.Interval = GetDelayToNextSlot(1);
            _currentTimeString = DateTime.Now.ToString("HH:mm");
            SetComponents();
        }

        private void ImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowTools();
        }

        private void TextBlockInfoClimat_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.infoclimat.fr/",
                UseShellExecute = true
            });
        }
        #endregion
    }
}