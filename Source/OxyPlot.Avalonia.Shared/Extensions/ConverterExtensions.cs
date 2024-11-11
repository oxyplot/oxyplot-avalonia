using Avalonia;

namespace OxyPlot.Avalonia.Extensions
{
    public static class ConverterExtensions
    {
        public static OxyRect ToOxyRect(this Rect rect)
        {
            return new OxyRect(rect.Left, rect.Top, rect.Width, rect.Height);
        }
        public static Rect ToRect(this OxyRect rect)
        {
            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }
    }
}
