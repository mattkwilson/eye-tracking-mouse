using System.Windows.Forms;
using EyeTrackingMouse.graphics;
using EyeTrackingMouse.Properties;

namespace EyeTrackingMouse
{
    class MyInputHandler : InputHandler
    {
        public MyInputHandler(EyeTrackingMouse trackingMouse, DisplayHandler displayHandler, 
        GraphicalOverlay graphicalOverlay)
        : base(trackingMouse, displayHandler, graphicalOverlay) { }

        protected override void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F17)
            {
                SnapCursor(Settings.Default.MainDisplayIndex);
            }

            if (!Settings.Default.IsScrollingEnabled)
            {
                if (e.KeyCode == Keys.F19)
                {
                    inputSimulator.Mouse.VerticalScroll(1);
                }
                else if (e.KeyCode == Keys.F20)
                {
                    inputSimulator.Mouse.VerticalScroll(-1);
                }

            }
        }

        protected override void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F17)
            {
                snapStopwatch.Reset();
            }
        }
        
    }
}
