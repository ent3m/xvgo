using System.Drawing;

namespace XVGO.Extensions;

public static class ColorExtensions
{
    extension(Color color)
    {
        public static Color CurrentColor => Color.FromName("CurrentColor");
    }
}