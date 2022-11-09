using System;
using System.Drawing;
using System.Windows.Forms;

namespace EyeTrackingMouse
{
    public struct Vector2
    {
        public double X;
        public double Y;

        public Vector2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    static class Utility
    {

        public static Point GetRelativeCursorPosition(Screen mainScreen, Point point)
        {            
            point.X -= mainScreen.Bounds.Location.X;
            point.Y -= mainScreen.Bounds.Location.Y;

            return point;
        }

        public static Vector2 TransformToVirtualScreenPosition(Vector2 point)
        {
            double ratioX = point.X / (double)Screen.PrimaryScreen.Bounds.Width;
            double ratioY = point.Y / (double)Screen.PrimaryScreen.Bounds.Height;
            
            point.X = ratioX * 65535;
            point.Y = ratioY * 65535;

            return point;
        }

        public static Vector2 TransformScreenPosition(Screen mainScreen, Screen outputScreen, Vector2 point)
        {
            double ratioX = point.X / (double)mainScreen.Bounds.Width;
            double ratioY = point.Y / (double)mainScreen.Bounds.Height;
            
            double screenPositionX = outputScreen.Bounds.Location.X;
            double screenPositionY = outputScreen.Bounds.Location.Y;

            double relativePositionX = ratioX * outputScreen.Bounds.Width;
            double relativePositionY = ratioY * outputScreen.Bounds.Height;

            point.X = screenPositionX + relativePositionX;
            point.Y = screenPositionY + relativePositionY;

            return point;
        }
    
        public static Vector2 TransformVirtualScreenPosition(Screen screen, Vector2 point)
        {
            return TransformVirtualScreenPosition(screen, screen, point);
        }

        public static Vector2 TransformVirtualScreenPosition(Screen mainScreen, Screen outputScreen, Vector2 point)
        {
            double ratioX = point.X / (double)mainScreen.Bounds.Width;
            double ratioY = point.Y / (double)mainScreen.Bounds.Height;

            double screenPositionX = (outputScreen.Bounds.Location.X - SystemInformation.VirtualScreen.Location.X) /
                (double)SystemInformation.VirtualScreen.Width * 65535;
            double screenPositionY = (outputScreen.Bounds.Location.Y - SystemInformation.VirtualScreen.Location.Y) /
                (double)SystemInformation.VirtualScreen.Height * 65535;

            double relativePositionX = ratioX * 65535 * Math.Abs(outputScreen.Bounds.Width / (double)SystemInformation.VirtualScreen.Width);
            double relativePositionY = ratioY * 65535 * Math.Abs(outputScreen.Bounds.Height / (double)SystemInformation.VirtualScreen.Height);

            point.X = screenPositionX + relativePositionX;
            point.Y = screenPositionY + relativePositionY;

            return point;
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, double factor)
        {
            return new Vector2((a.X + (b.X - a.X) * factor), (a.Y + (b.Y - a.Y) * factor));
        }
        public static Point Lerp(Point a, Vector2 b, double factor)
        {
            return new Point((int)(a.X + (b.X - a.X) * factor), (int)(a.Y + (b.Y - a.Y) * factor));
        }

        public static double Distance(Vector2 a, Vector2 b)
        {
            return Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }
    }
}
