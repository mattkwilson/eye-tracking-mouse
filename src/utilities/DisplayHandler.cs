using EyeTrackingMouse.Properties;
using System;
using System.Windows.Forms;

namespace EyeTrackingMouse
{
    public delegate void DisplayEvent();
    class DisplayHandler
    {
        public event DisplayEvent MainDisplayUpdated;
        public event DisplayEvent NumberOfDisplaysChanged;

        private int numberOfDisplays;
        private Timer displayTimer;
        private int mainDisplayIndex;
        private Screen mainDisplay;
        public Screen MainDisplay 
        {
            get { return mainDisplay; }
        }
       
        public DisplayHandler()
        {
            if (Screen.AllScreens.Length <= Settings.Default.MainDisplayIndex)
            {
                mainDisplay = Screen.AllScreens[0];
                Settings.Default.MainDisplayIndex = 0;
            }
            else
            {
                mainDisplay = Screen.AllScreens[Settings.Default.MainDisplayIndex];
            }
            numberOfDisplays = Screen.AllScreens.Length;
            mainDisplayIndex = Settings.Default.MainDisplayIndex;

            displayTimer = new Timer();
            displayTimer.Interval = 5000;
            displayTimer.Tick += DisplayCheck;
            displayTimer.Enabled = true;
        }

        public void DisplayCheck(object sender, EventArgs e)
        {
            if (numberOfDisplays != Screen.AllScreens.Length)
            {
                if (Screen.AllScreens.Length <= Settings.Default.MainDisplayIndex)
                {
                    mainDisplay = Screen.AllScreens[0];
                    Settings.Default.MainDisplayIndex = 0;
                    UpdateMainDisplay();
                }
                numberOfDisplays = Screen.AllScreens.Length;
                NumberOfDisplaysChanged?.Invoke();
            }

            if (mainDisplayIndex != Settings.Default.MainDisplayIndex)
            {
                UpdateMainDisplay();
            }
        }

        public void UpdateMainDisplay()
        {                                
            mainDisplay = Screen.AllScreens[Settings.Default.MainDisplayIndex];
            mainDisplayIndex = Settings.Default.MainDisplayIndex;
            MainDisplayUpdated?.Invoke();
        }

        public void Dispose()
        {
            displayTimer.Dispose();
        }
        
    }
}
