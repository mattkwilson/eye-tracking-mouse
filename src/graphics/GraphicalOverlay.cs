using EyeTrackingMouse.Properties;
using GameOverlay.Drawing;
using GameOverlay.Windows;

namespace EyeTrackingMouse.graphics
{    
    class GraphicalOverlay : GraphicalOverlayBase 
    {
        private EyeTrackingMouse trackingMouse;

        private Rectangle scrollUpRectangle;
        public Rectangle ScrollUpRectangle {
            get { return scrollUpRectangle; }
        }
        private Rectangle scrollDownRectangle;
        public Rectangle ScrollDownRectangle
        {
            get { return scrollDownRectangle; }
        }
        private Rectangle scrollLeftRectangle;
        public Rectangle ScrollLeftRectangle
        {
            get { return scrollLeftRectangle; }
        }
        private Rectangle scrollRightRectangle;
        public Rectangle ScrollRightRectangle => scrollRightRectangle;
        private Triangle[] scrollArrows;

        private SudokuGrid sudokuGrid;
        private Point crosshairPosition;

        public GraphicalOverlay(EyeTrackingMouse trackingMouse, System.Drawing.Rectangle bounds) : base(bounds)
        {
            this.trackingMouse = trackingMouse; 
            sudokuGrid = new SudokuGrid(bounds);
            UpdateRects();    
        }

        public override void Create()
        {
            base.Create();
            sudokuGrid.Create();
            sudokuGrid.Hide();
        }

        public SudokuGrid GetSudokuGrid()
        {
            return sudokuGrid;
        }

        public override void Dispose()
        {
            base.Dispose();
            sudokuGrid.Dispose();
        }

        private void UpdateRects()
        {
            switch (OverlaySettings.Default.VerticalScrollIndicatorPosition)
            {
                case 0:

                    scrollUpRectangle = new Rectangle(0,
                                                      0,
                                                      ScrollRectangleSize,
                                                      ScrollRectangleSize);
                    scrollDownRectangle = new Rectangle(0,
                                                        bounds.Height - ScrollRectangleSize,
                                                        ScrollRectangleSize,
                                                        bounds.Height);
                    break;
                case 1:

                    scrollUpRectangle = new Rectangle(bounds.Width / 2f - ScrollRectangleSize / 2f,
                                                      0,
                                                      bounds.Width / 2f + ScrollRectangleSize / 2f,
                                                      ScrollRectangleSize);
                    scrollDownRectangle = new Rectangle(bounds.Width / 2f - ScrollRectangleSize / 2f,
                                                        bounds.Height - ScrollRectangleSize,
                                                        bounds.Width / 2f + ScrollRectangleSize / 2f,
                                                        bounds.Height);
                    break;
                case 2:

                    scrollUpRectangle = new Rectangle(bounds.Width - ScrollRectangleSize,
                                                      0,
                                                      bounds.Width,
                                                      ScrollRectangleSize);
                    scrollDownRectangle = new Rectangle(bounds.Width - ScrollRectangleSize,
                                                        bounds.Height - ScrollRectangleSize,
                                                        bounds.Width,
                                                        bounds.Height);
                    break;
            }

            scrollLeftRectangle = new Rectangle(0,
                                                bounds.Height / 2f - ScrollRectangleSize / 2f,
                                                ScrollRectangleSize,
                                                bounds.Height / 2f + ScrollRectangleSize / 2f);
            scrollRightRectangle = new Rectangle(bounds.Width - ScrollRectangleSize,
                                                 bounds.Height / 2f - ScrollRectangleSize / 2f,
                                                 bounds.Width,
                                                 bounds.Height / 2f + ScrollRectangleSize / 2f);

            scrollArrows = new Triangle[4];
            // up
            Point trianglePointA = new Point(scrollUpRectangle.Left + scrollUpRectangle.Width / 3f, scrollUpRectangle.Bottom - scrollUpRectangle.Height / 3f);
            Point trianglePointB = new Point(scrollUpRectangle.Left + scrollUpRectangle.Width * 2 / 3f, scrollUpRectangle.Bottom - scrollUpRectangle.Height / 3f);
            Point trianglePointC = new Point(scrollUpRectangle.Left + scrollUpRectangle.Width / 2f, scrollUpRectangle.Top + scrollUpRectangle.Height / 3f);
            scrollArrows[0] = new Triangle(trianglePointA, trianglePointB, trianglePointC);
            // down
            trianglePointA = new Point(scrollDownRectangle.Left + scrollDownRectangle.Width / 3f, scrollDownRectangle.Top + scrollDownRectangle.Height / 3f);
            trianglePointB = new Point(scrollDownRectangle.Left + scrollDownRectangle.Width * 2 / 3f, scrollDownRectangle.Top + scrollDownRectangle.Height / 3f);
            trianglePointC = new Point(scrollDownRectangle.Left + scrollDownRectangle.Width / 2f, scrollDownRectangle.Bottom - scrollDownRectangle.Height / 3f);
            scrollArrows[1] = new Triangle(trianglePointA, trianglePointB, trianglePointC);
            // left
            trianglePointA = new Point(scrollLeftRectangle.Left + scrollLeftRectangle.Width * 2 / 3f, scrollLeftRectangle.Top + scrollLeftRectangle.Height / 3f);
            trianglePointB = new Point(scrollLeftRectangle.Left + scrollLeftRectangle.Width * 2 / 3f, scrollLeftRectangle.Bottom - scrollLeftRectangle.Height / 3f);
            trianglePointC = new Point(scrollLeftRectangle.Left + scrollLeftRectangle.Width / 3f, scrollLeftRectangle.Top + scrollLeftRectangle.Height / 2f);
            scrollArrows[2] = new Triangle(trianglePointA, trianglePointB, trianglePointC);
            // right                
            trianglePointA = new Point(scrollRightRectangle.Left + scrollRightRectangle.Width / 3f, scrollRightRectangle.Top + scrollRightRectangle.Height / 3f);
            trianglePointB = new Point(scrollRightRectangle.Left + scrollRightRectangle.Width / 3f, scrollRightRectangle.Bottom - scrollRightRectangle.Height / 3f);
            trianglePointC = new Point(scrollRightRectangle.Left + scrollRightRectangle.Width * 2 / 3f, scrollRightRectangle.Top + scrollRightRectangle.Height / 2f);
            scrollArrows[3] = new Triangle(trianglePointA, trianglePointB, trianglePointC);

            // trackingMouse.UpdateInteractors();
        }

        public void ShowSudoku()
        {
            sudokuGrid.Show();
        }

        public void HideSudoku()
        {
            sudokuGrid.Hide();
        }

        public void SetCrosshairPosition(float x, float y)
        {
            crosshairPosition = new Point(x, y);
        }

        protected override void SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            base.SetupGraphics(sender, e);
            var g = e.Graphics;
            brushes["scrollRectangle"] = g.CreateSolidBrush(255, 255, 255, 125);
            brushes["scrollArrow"] = g.CreateSolidBrush(255, 255, 255, 125);
            UpdateRects();
        }

        protected override void DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            var g = e.Graphics;

            g.ClearScene(brushes["transparent"]);

            if (OverlaySettings.Default.IsCrosshairVisible)
            {
                g.FillCircle(brushes["black"], crosshairPosition, CrosshairCentreSize + 0.6f);
                g.FillCircle(brushes["white"], crosshairPosition, CrosshairCentreSize);
                if (!OverlaySettings.Default.IsSimpleCrosshair)
                {
                    g.DrawCircle(brushes["black"], crosshairPosition, CrosshairRadius, CrosshairStroke + 1.2f);
                    g.DrawCircle(brushes["white"], crosshairPosition, CrosshairRadius, CrosshairStroke);
                }
            }
            if (Settings.Default.IsScrollingEnabled)
            {
                switch (trackingMouse.FocusedInteractor)
                {
                    case InteractorId.ScrollUp:
                        g.FillRectangle(brushes["scrollRectangle"], scrollUpRectangle);
                        break;
                    case InteractorId.ScrollDown:
                        g.FillRectangle(brushes["scrollRectangle"], scrollDownRectangle);
                        break;
                    case InteractorId.ScrollLeft:
                        g.FillRectangle(brushes["scrollRectangle"], scrollLeftRectangle);
                        break;
                    case InteractorId.ScrollRight:
                        g.FillRectangle(brushes["scrollRectangle"], scrollRightRectangle);
                        break;
                }
                
                g.FillTriangle(brushes["scrollArrow"], scrollArrows[0]);
                g.FillTriangle(brushes["scrollArrow"], scrollArrows[1]);

                g.FillTriangle(brushes["scrollArrow"], scrollArrows[2]);
                g.FillTriangle(brushes["scrollArrow"], scrollArrows[3]);
            }
        }
    }
}
