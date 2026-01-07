using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace KeyPad
{
    /// <summary>
    /// Represents a custom keypad window for user input.
    /// Provides alignment options and notifies property changes.
    /// </summary>
    public partial class Keypad : Window, INotifyPropertyChanged
    {
        public enum KeypadHorizontalAlignment
        {
            Left,
            Center,
            Right
        }

        public enum KeypadVerticalAlignment
        {
            Top,         
            Bottom
        }

        #region Fields       
        public string? Result { get => field; private set { field = value; OnPropertyChanged("Result"); } }

        private KeypadHorizontalAlignment? _horizontalAlignment;
        private KeypadVerticalAlignment? _verticalAlignment;
        private Control? _parent;
        #endregion

        #region Ctor
        public Keypad(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            DataContext = this;
        }

        public Keypad(Window owner, Control parent, KeypadHorizontalAlignment? horizontalAlignment, KeypadVerticalAlignment? verticalAlignment)
        {
            InitializeComponent();
            Owner = owner;
            DataContext = this;
            _parent = parent;
            _horizontalAlignment = horizontalAlignment;
            _verticalAlignment = verticalAlignment;                     
        }
        #endregion

        /// <summary>
        ///     Displays the custom keypad dialog for user input and updates the specified <see cref="TextBox"/> with the result.
        /// </summary>
        /// <param name="owner">
        ///     The parent <see cref="Window"/> that will own the keypad dialog.
        /// </param>
        /// <param name="textBox">
        ///     The <see cref="TextBox"/> control whose text will be set to the value entered by the user if the dialog is accepted.
        /// </param>
        /// <param name="horizontalAlignment">
        ///     The optional horizontal alignment (<see cref="KeypadHorizontalAlignment"/>) for positioning the keypad relative to the parent control.
        /// </param>
        /// <param name="verticalAlignment">
        ///     The optional vertical alignment (<see cref="KeypadVerticalAlignment"/>) for positioning the keypad relative to the parent control.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the user confirmed the input and the <see cref="TextBox"/> was updated; otherwise, <c>false</c>.
        /// </returns>
        public static bool Show(Window owner, TextBox textBox, KeypadHorizontalAlignment? horizontalAlignment, KeypadVerticalAlignment? verticalAlignment)
        {
            var keyPad = new Keypad(owner, textBox, horizontalAlignment, verticalAlignment);
            if (keyPad.ShowDialog() == true)
            {
                textBox.Text = keyPad.Result;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Positions the keypad window relative to its parent control according to the specified horizontal
        ///     and vertical alignment settings.
        ///     <para>
        ///         If a parent control is defined, this method calculates the absolute screen position of the parent
        ///         and sets the <see cref="Window.Left"/> and <see cref="Window.Top"/> properties of the keypad window
        ///         based on the selected <see cref="KeypadHorizontalAlignment"/> and <see cref="KeypadVerticalAlignment"/> values.
        ///     </para>
        ///     <para>
        ///         - For horizontal alignment:
        ///             <list type="bullet">
        ///                 <item><description><c>Left</c>: aligns the left edge of the keypad with the parent.</description></item>
        ///                 <item><description><c>Center</c>: centers the keypad horizontally over the parent.</description></item>
        ///                 <item><description><c>Right</c>: aligns the right edge of the keypad with the parent.</description></item>
        ///             </list>
        ///         - For vertical alignment:
        ///             <list type="bullet">
        ///                 <item><description><c>Top</c>: aligns the top edge of the keypad with the parent.</description></item>
        ///                 <item><description><c>Bottom</c>: aligns the bottom edge of the keypad with the bottom of the parent.</description></item>
        ///             </list>
        ///     </para>
        ///     If no parent control is set, the method exits without modifying the window position.
        /// </summary>
        private void SetControlPositions()
        {
            if (_parent is null) { return; }

            // Get the absolute position of the parent control on the screen
            var parentPosition = _parent.PointToScreen(new Point(0, 0));

            if (_horizontalAlignment.HasValue)
            {
                switch (_horizontalAlignment.Value)
                {
                    case KeypadHorizontalAlignment.Left:
                        Left = parentPosition.X;
                        break;
                    case KeypadHorizontalAlignment.Center:
                        Left = parentPosition.X + (_parent.ActualWidth / 2);
                        break;
                    case KeypadHorizontalAlignment.Right:
                        Left = parentPosition.X + _parent.ActualWidth;
                        break;
                }
            }
            if (_verticalAlignment.HasValue)
            {
                switch (_verticalAlignment.Value)
                {
                    case KeypadVerticalAlignment.Top:
                        Top = parentPosition.Y;
                        break;
                    case KeypadVerticalAlignment.Bottom:
                        Top = parentPosition.Y + _parent.ActualHeight;
                        break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            switch (button.CommandParameter.ToString())
            {
                case "ESC":
                    DialogResult = false;
                    break;

                case "RETURN":
                    DialogResult = true;
                    break;

                case "BACK":
                    if (Result?.Length > 0)
                        Result = Result.Remove(Result.Length - 1);
                    break;

                default:
                    Result += button.Content.ToString();
                    break;
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region IHM
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetControlPositions();            
        }
        #endregion
    }
}
