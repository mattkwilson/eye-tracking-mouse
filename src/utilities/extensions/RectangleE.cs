using Windows.UI.Xaml.Media.Animation;
using Tobii.StreamEngine;

namespace EyeTrackingMouse.extensions
{
    public static class RectangleE
    {
        public static bool Contains(this GameOverlay.Drawing.Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X < bounds.Right
                 && point.Y >= bounds.Top && point.Y < bounds.Bottom;
        }
    }
}