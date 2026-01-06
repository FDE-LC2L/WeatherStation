using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace WeatherStation.UserControls
{
    #region Converters
    public class VerticalOrientationToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Orientation)value).Equals(Orientation.Vertical) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Orientation)value).Equals(Orientation.Vertical) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class HorizontalOrientationToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Orientation)value).Equals(Orientation.Horizontal) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Orientation)value).Equals(Orientation.Horizontal) ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
    #endregion

    /// <summary>
    /// Logique d'interaction pour ImageButton.xaml
    /// </summary>
    public partial class ImageButton : Button
    {

        #region Custom Properties
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string),
                                                                 typeof(ImageButton), new FrameworkPropertyMetadata("Button text"));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); SetComponents(); }
        }

        public static readonly DependencyProperty ImageSrcProperty = DependencyProperty.Register("ImageSrc", typeof(ImageSource),
                                                                    typeof(ImageButton), new FrameworkPropertyMetadata(null));

        public ImageSource ImageSrc
        {
            get { return (ImageSource)GetValue(ImageSrcProperty); }
            set { SetValue(ImageSrcProperty, value); SetComponents(); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(int),
                                                                         typeof(ImageButton), new FrameworkPropertyMetadata(0));

        public int CornerRadius
        {
            get { return (int)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); SetComponents(); }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation),
                                                                         typeof(ImageButton), new FrameworkPropertyMetadata(Orientation.Vertical));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); SetComponents(); }
        }

        public static readonly DependencyProperty CatchEnterKeyProperty = DependencyProperty.Register("CatchEnterKey", typeof(bool),
                                                                 typeof(ImageButton), new FrameworkPropertyMetadata(true));

        [Description("If true then the enter keys will not trigger the OnClick event.\rDefault value is true")]
        public bool CatchEnterKey
        {
            get { return (bool)GetValue(CatchEnterKeyProperty); }
            set { SetValue(CatchEnterKeyProperty, value); }
        }

        #endregion

        public ImageButton()
        {
            InitializeComponent();
            PreviewKeyDown += ImageButton_PreviewKeyDown;
            SetComponents();
        }

        private void ImageButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CatchEnterKey)
            {
                e.Handled = true;
            }
        }

        private void SetComponents()
        {


        }


    }
}
