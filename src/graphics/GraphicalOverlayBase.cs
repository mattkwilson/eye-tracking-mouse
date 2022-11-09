using System;
using System.Collections.Generic;
using GameOverlay.Drawing;
using GameOverlay.Windows;

namespace EyeTrackingMouse.graphics
{
    internal abstract class GraphicalOverlayBase
    {
        public const int CrosshairRadius = 30;
        protected const float CrosshairStroke = 1.4f;
        protected const float CrosshairCentreSize = 1.6f;
        public const int ScrollRectangleSize = 100;
        
        protected GraphicsWindow window;
        protected Graphics graphics;
        protected Dictionary<string, SolidBrush> brushes;
        protected System.Drawing.Rectangle bounds;
        protected bool setToRefresh;

        public GraphicalOverlayBase(System.Drawing.Rectangle bounds)
        {
            this.bounds = bounds;

            brushes = new Dictionary<string, SolidBrush>();
                        
            graphics = new Graphics()
            {                
                PerPrimitiveAntiAliasing = true,                                                
                WindowHandle = IntPtr.Zero
            };
            
            window = new GraphicsWindow(graphics)
            {
                IsTopmost = true,
                IsVisible = true,
                FPS = 60,
                X = bounds.Location.X,
                Y = bounds.Location.Y,
                Width = bounds.Width,
                Height = bounds.Height
            };

            window.SetupGraphics += SetupGraphics;
            window.DestroyGraphics += DestroyGraphics;
            window.DrawGraphics += DrawGraphics;
            window.DrawGraphics += RefreshGraphics;
        }

        public virtual void Create()
        {
            window.Create();
        }

        public void Show()
        {
            window.Show();
        }

        public void Hide()
        {
            window.Hide();
        }

        public void SetSize(System.Drawing.Rectangle bounds)
        {
            this.bounds = bounds;            
            window.Resize(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            window.Recreate();
        }

        public void Refresh()
        {
            setToRefresh = true;
        }

        protected virtual void RefreshGraphics(object sender, DrawGraphicsEventArgs e)
        {
            if (!setToRefresh) return;
            graphics.Destroy();            
            graphics.Setup();            
            setToRefresh = false;
        }

        public virtual void Dispose()
        {
            window.Dispose();
            graphics.Dispose();
        }

        protected virtual void SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            var g = e.Graphics;            
            brushes["black"] = g.CreateSolidBrush(0, 0, 0);
            brushes["white"] = g.CreateSolidBrush(255, 255, 255);
            brushes["transparent"] = g.CreateSolidBrush(0, 0, 0, 0);
        }

        protected virtual void DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            foreach (var brush in brushes)
                brush.Value.Dispose();
        }

        protected abstract void DrawGraphics(object sender, DrawGraphicsEventArgs e);
    }
}