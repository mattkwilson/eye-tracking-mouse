using EyeTrackingMouse.Properties;
using System;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace EyeTrackingMouse
{
    public delegate void TrayIconEvent();
    class SystemTrayIcon
    {
        public event TrayIconEvent ExitEvent;

        private NotifyIcon notifyIcon;

        private ToolStripMenuItem snapMenuItem;
        private ToolStripMenuItem crosshairMenuItem;
        private ToolStripMenuItem visibleMenuItem;
        private ToolStripMenuItem headTrackingItem;
        private ToolStripMenuItem[] displayMenuItems;

        private DisplayHandler displayHandler;
        public SystemTrayIcon(DisplayHandler displayHandler)
        {
            this.displayHandler = displayHandler;
            displayHandler.NumberOfDisplaysChanged += UpdateContextMenu;

            displayMenuItems = new ToolStripMenuItem[Screen.AllScreens.Length];
            for (int i = 0; i < displayMenuItems.Length; i++)
            {
                displayMenuItems[i] = new ToolStripMenuItem("Display " + (i + 1), null, ChangeDisplay);
                if (i == Settings.Default.MainDisplayIndex)                
                    displayMenuItems[i].Checked = true;                
            }

            snapMenuItem = new ToolStripMenuItem("Snap", null, SnapOnClick);
            snapMenuItem.Checked = Settings.Default.IsSnapEnabled;
            crosshairMenuItem = new ToolStripMenuItem("Simple Crosshair", null, SimpleCrosshairOnClick);
            crosshairMenuItem.Checked = OverlaySettings.Default.IsSimpleCrosshair;
            visibleMenuItem = new ToolStripMenuItem("Visible", null, ToggleCrosshair);
            visibleMenuItem.Checked = OverlaySettings.Default.IsCrosshairVisible;
            headTrackingItem = new ToolStripMenuItem("Head Tracking", null, ToggleHeadTracking);
            headTrackingItem.Checked = Settings.Default.IsHeadTrackingEnabled;
            notifyIcon = new NotifyIcon()
            {
                Icon = Resources.AppIcon,
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true,

            };
            notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                headTrackingItem,
                snapMenuItem,
                new ToolStripMenuItem("Crosshair", null, new ToolStripMenuItem[] { visibleMenuItem, crosshairMenuItem }),
                new ToolStripMenuItem("Set Display", null, displayMenuItems),
                new ToolStripMenuItem("Exit", null, Exit)
            });
            notifyIcon.ContextMenuStrip.AutoClose = true;
            notifyIcon.MouseClick += NotifyIconOnClick;
        }

        public void ChangeDisplay(object sender, EventArgs e)
        {
            ToolStripMenuItem displayMenuItem = sender as ToolStripMenuItem;            
            if (!displayMenuItem.Checked)
            {
                for (int i = 0; i < displayMenuItems.Length; i++)
                {
                    if (displayMenuItems[i] == displayMenuItem)
                    {
                        displayMenuItems[i].Checked = true;
                        Settings.Default.MainDisplayIndex = i;
                    } else
                        displayMenuItems[i].Checked = false;                    
                }
                displayHandler.UpdateMainDisplay();
            }
        }

        public void UpdateContextMenu()
        {
            displayMenuItems = new ToolStripMenuItem[Screen.AllScreens.Length];
            for (int i = 0; i < displayMenuItems.Length; i++)
            {
                displayMenuItems[i] = new ToolStripMenuItem("Display " + (i + 1), null, ChangeDisplay);
                if (i == Settings.Default.MainDisplayIndex)                
                    displayMenuItems[i].Checked = true;                
            }

            notifyIcon.ContextMenuStrip.Items.Clear();
            notifyIcon.ContextMenuStrip.Items.AddRange(new ToolStripItem[] {
                        headTrackingItem,
                        snapMenuItem,
                        new ToolStripMenuItem("Crosshair", null, new ToolStripMenuItem[] { visibleMenuItem, crosshairMenuItem }),
                        new ToolStripMenuItem("Set Display", null, displayMenuItems),
                        new ToolStripMenuItem("Exit", null, Exit)
                    });
        }

        public void Dispose()
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        void SnapOnClick(object sender, EventArgs e)
        {
            Settings.Default.IsSnapEnabled = !Settings.Default.IsSnapEnabled;
            snapMenuItem.Checked = Settings.Default.IsSnapEnabled;
        }

        void SimpleCrosshairOnClick(object sender, EventArgs e)
        {
            OverlaySettings.Default.IsSimpleCrosshair = !OverlaySettings.Default.IsSimpleCrosshair;
            crosshairMenuItem.Checked = OverlaySettings.Default.IsSimpleCrosshair;
        }

        void ToggleCrosshair(object sender, EventArgs e)
        {
            OverlaySettings.Default.IsCrosshairVisible = !OverlaySettings.Default.IsCrosshairVisible;
            visibleMenuItem.Checked = OverlaySettings.Default.IsCrosshairVisible;
        }

        void ToggleHeadTracking(object sender, EventArgs e)
        {
            Settings.Default.IsHeadTrackingEnabled = !Settings.Default.IsHeadTrackingEnabled;
            headTrackingItem.Checked = Settings.Default.IsHeadTrackingEnabled;
        }

        void NotifyIconOnClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
            {
                snapMenuItem.Checked = Settings.Default.IsSnapEnabled;
                crosshairMenuItem.Checked = OverlaySettings.Default.IsSimpleCrosshair;
                visibleMenuItem.Checked = OverlaySettings.Default.IsCrosshairVisible;
            }
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo notifyShowContext = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                notifyShowContext.Invoke(notifyIcon, null);
            }
        }

        void Exit(object sender, EventArgs e)
        {
            Dispose();
            ExitEvent?.Invoke();
        }

    }
}
