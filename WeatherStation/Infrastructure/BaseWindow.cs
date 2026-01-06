
using System.Windows;

namespace WeatherStation.Infrastructure
{
    public class BaseWindow : Window
    {

        #region Fields
        protected bool _FirstInit;
        protected static AppSettingsManager AppParameters { get => AppSettingsManager.Instance; }

        protected bool _DataLoading;
        #endregion

        #region Ctor
        public BaseWindow() : base()
        {
            Initialized += CustomWindow_Initialized;
            Closed += CustomWindow_Closed;
        }

        public BaseWindow(Window owner) : this()
        {
            Owner = owner;
        }
        #endregion

        protected virtual void FirstInit()
        {

        }

        protected virtual void DelayedFirstInit()
        {

        }

        protected virtual void SetComponents()
        {

        }

        protected async void SetDataLoading(bool IsDataLoading)
        {
            await Dispatcher.BeginInvoke(() =>
            {
                _DataLoading = IsDataLoading;
                SetComponents();
            });
        }

        /// <summary>
        /// Set the Windows style and the Resize Mode depending on whether the application is in debug or release
        /// </summary>
        protected virtual void SetWindowsMode()
        {
#if DEBUG
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
#else
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
#endif
        }

        private void StartDelayedFirstInitTimer()
        {
            // Create a timer with a two second interval.
            var aTimer = new System.Timers.Timer(1500);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender is object)
            {
                ((System.Timers.Timer)sender).Enabled = false;
            }
            Dispatcher.Invoke(() =>
            {
                DelayedFirstInit();
            });

        }

        private void CustomWindow_Initialized(object? sender, System.EventArgs e)
        {
            if (!_FirstInit)
            {
             //   WindowHelper.RestorePosition(this);
                FirstInit();
                StartDelayedFirstInitTimer();
                SetComponents();
                _FirstInit = true;
            }
        }

        private void CustomWindow_Closed(object? sender, System.EventArgs e)
        {
           // WindowHelper.SavePosition(this);
        }
    }
}
