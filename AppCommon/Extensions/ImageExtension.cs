using System.Windows.Controls;

namespace AppCommon.Extension
{
    public static class ImageExtension
    {
        private static readonly Dictionary<Image, bool> _enabledFlag = new();


        extension(Image image)
        {
            /// <summary>
            /// Extension property <c>Enabled</c> for <see cref="Image"/> instances.
            /// This property allows enabling or disabling an <see cref="Image"/> control and adjusts its opacity accordingly.
            /// When set to <c>true</c>, the image is enabled and its opacity is set to 1.0.
            /// When set to <c>false</c>, the image is disabled and its opacity is set to 0.4.
            /// The enabled state is tracked internally for each <see cref="Image"/> instance.
            /// </summary>
            /// <param name="image">The <see cref="Image"/> control to extend.</param>
            /// <returns>
            /// <c>true</c> if the image is enabled; otherwise, <c>false</c>.
            /// </returns>
            public bool Enabled
            {
                get => _enabledFlag.TryGetValue(image, out var value) && value;
                set
                {
                    _enabledFlag[image] = value;
                    image.IsEnabled = value;
                    image.Opacity = value ? 1d : 0.4d;
                }
            }
        }


    }
}