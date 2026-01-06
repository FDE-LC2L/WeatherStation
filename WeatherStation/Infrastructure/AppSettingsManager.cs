using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using WeatherStation.Geo;

namespace WeatherStation.Infrastructure
{
    public partial class AppSettingsManager
    {
        public partial class LocalSettings : ObservableObject
        {
            [ObservableProperty]
            private string _lastUsedNetworkInterfaceId = string.Empty;

            [ObservableProperty]
            private string _lastUsedPostalCode = string.Empty;

            [ObservableProperty]
            private City? _currentCity = null;


            /// <summary>
            /// Constructor => Set default parameter values here.
            /// </summary>
            public LocalSettings()
            {

            }
        }


        #region AppSettingsManager Singleton Management

        private static readonly Lazy<AppSettingsManager> _instance = new(() => new AppSettingsManager());

        /// <summary>
        /// Gets the single instance of the AppSettingsManager.
        /// </summary>
        public static AppSettingsManager Instance => _instance.Value;

        #endregion


        #region Fields
        private const string AppSettingFileName = "Settings.settings";
        private string _appDataFolder = string.Empty;
        private string _appSettingsFolder = string.Empty;
        private string _appSettingCompleteFileName = string.Empty;
        public LocalSettings Settings { get; private set; } = null!;
        public string Company { get; private set; } = string.Empty;
        public string Copyright { get; private set; } = string.Empty;
        public string Product { get; private set; } = string.Empty;
        public string AppTitle { get; private set; } = string.Empty;
        public string FileVersion { get; private set; } = string.Empty;
        public string AppConfiguration { get; private set; } = string.Empty;
        public DateTime AppCompilationDate { get; private set; }
        #endregion

        private AppSettingsManager()
        {
            LoadAssemblyAttributes(Assembly.GetExecutingAssembly());
        }

        public void LoadAssemblyAttributes(Assembly assembly)
        {
            CustomAttributeData? attribute;
            var customAttributes = assembly.CustomAttributes;
            attribute = customAttributes.FirstOrDefault(ca => ca.AttributeType.Equals(typeof(AssemblyCompanyAttribute)));
            if (attribute is object)
            {
                Company = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            }
            attribute = customAttributes.FirstOrDefault(ca => ca.AttributeType.Equals(typeof(AssemblyCopyrightAttribute)));
            if (attribute is object)
            {
                Copyright = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            }
            attribute = customAttributes.FirstOrDefault(ca => ca.AttributeType.Equals(typeof(AssemblyTitleAttribute)));
            if (attribute is object)
            {
                AppTitle = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            }
            attribute = customAttributes.FirstOrDefault(ca => ca.AttributeType.Equals(typeof(AssemblyFileVersionAttribute)));
            if (attribute is object)
            {
                FileVersion = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            }
            attribute = customAttributes.FirstOrDefault(ca => ca.AttributeType.Equals(typeof(AssemblyConfigurationAttribute)));
            if (attribute is object)
            {
                AppConfiguration = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            }
            attribute = customAttributes.FirstOrDefault(ca => ca.AttributeType.Equals(typeof(AssemblyProductAttribute)));
            if (attribute is object)
            {
                Product = attribute.ConstructorArguments[0].Value?.ToString() ?? string.Empty;
            }
            AppCompilationDate = File.GetLastWriteTime(assembly.Location);
            SetAppParamsFolder();
        }

        private void SetAppParamsFolder()
        {
            _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _appSettingsFolder = Path.Combine(_appDataFolder, Company, AppTitle);
            _appSettingCompleteFileName = Path.Combine(_appSettingsFolder, AppSettingFileName);
            Directory.CreateDirectory(_appSettingsFolder);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var json = File.Exists(_appSettingCompleteFileName) ? File.ReadAllText(_appSettingCompleteFileName) : null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var settings = JsonSerializer.Deserialize<LocalSettings>(json);
                    if (settings is object)
                    {
                        Settings = settings;
                        Settings.PropertyChanged += SettingsPropertyChanged;
                    }
                }
                catch
                {
                    // if a possible error occurs during deserialization, the application parameters
                    // are recreated with the default values but no exception is thrown.
                    Settings = new LocalSettings();
                    Settings.PropertyChanged += SettingsPropertyChanged;
                }
            }
            else
            {
                Settings = new LocalSettings();
                Settings.PropertyChanged += SettingsPropertyChanged;
            }

        }

        private void SettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (Settings is object)
            {
                var json = JsonSerializer.Serialize(Settings!);
                File.WriteAllText(_appSettingCompleteFileName, json);
            }

        }

    }
}
