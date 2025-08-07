using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace OLP.AutoConnector.Resources
{
    public static class Colors
    {
        public static Brush AlternatingRowColor { get => new SolidColorBrush(Color.FromRgb(245, 245, 245)); } //White smoke
        public static Brush TextBlockForegroundInfoColor { get => new SolidColorBrush(Color.FromRgb(65, 105, 225)); } //Royal Blue
        public static Brush ComboBoxBackgroundDefaultColor { get => new SolidColorBrush(Color.FromRgb(247, 247, 247)); }
        public static Brush ComboBoxBorderDefaultColor { get => new SolidColorBrush(Color.FromRgb(204, 204, 204)); }
    }
}
