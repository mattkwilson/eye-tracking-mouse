using System;
using System.Drawing;
using EyeTrackingMouse.Properties;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using Font = GameOverlay.Drawing.Font;
using Point = GameOverlay.Drawing.Point;
using SolidBrush = GameOverlay.Drawing.SolidBrush;

namespace EyeTrackingMouse.graphics
{
    class SudokuGrid : GraphicalOverlayBase
    {
        private const int GridSize = 3;        
        private const string FontFamilyName = "arial";
        
        private Font gridNumberFont;

        private SolidBrush lineBrush;
        private SolidBrush secondaryLineBrush;        
        private SolidBrush backgroundBrush;
        private SolidBrush textBrush;

        private Geometry bigGridGeometry;
        private Geometry smallGridGeometry;

        private Point[] numberPositions;        
        
        private float screenWidth;
        private float screenHeight;

        private bool refreshGrid;

        public SudokuGrid(System.Drawing.Rectangle bounds) : base(bounds)
        {
            refreshGrid = true;
        }

        protected override void RefreshGraphics(object sender, DrawGraphicsEventArgs e)
        {
            if (!setToRefresh) return;
            base.RefreshGraphics(sender, e);
            refreshGrid = true;
        }

        protected override void SetupGraphics(object sender, SetupGraphicsEventArgs e)
        {
            int numGridColumns = SudokuGridSettings.Default.NumberOfColumns;
            int numGridRows = SudokuGridSettings.Default.NumberOfRows;
            screenWidth = bounds.Width;
            screenHeight = bounds.Height;
            float cellWidth = screenWidth / numGridColumns;
            float cellHeight = screenHeight / numGridRows;
            numberPositions = new Point[numGridColumns * numGridRows];

            var g = e.Graphics;
            
            gridNumberFont = g.CreateFont(FontFamilyName, SudokuGridSettings.Default.FontSize);
            lineBrush = g.CreateSolidBrush(0, 0, 0, SudokuGridSettings.Default.StrokeOpacity);
            secondaryLineBrush = g.CreateSolidBrush(0, 0, 0, SudokuGridSettings.Default.SecondaryStrokeOpacity);
            textBrush = g.CreateSolidBrush(0, 0, 0, SudokuGridSettings.Default.TextOpacity);
            backgroundBrush = g.CreateSolidBrush(1, 1, 1, SudokuGridSettings.Default.BackgroundOpacity);

            bigGridGeometry = g.CreateGeometry();
            for (int x = 0; x < numGridColumns; x++)
            {
                float startX = x * cellWidth;
                float startY = 0;
                float endX = startX;
                float endY = screenHeight;
                bigGridGeometry.BeginFigure(new Point(startX, startY));
                bigGridGeometry.AddPoint(new Point(endX, endY));
                bigGridGeometry.EndFigure(false);
            }

            for (int y = 0; y < numGridRows; y++)
            {
                float startX = 0;
                float startY = y * cellHeight;
                float endX = screenWidth;
                float endY = startY;
                bigGridGeometry.BeginFigure(new Point(startX, startY));
                bigGridGeometry.AddPoint(new Point(endX, endY));
                bigGridGeometry.EndFigure(false);
            }
            bigGridGeometry.Close();

            smallGridGeometry = g.CreateGeometry();
            float digitWidth = SudokuGridSettings.Default.FontSize * 4 / 7f;
            float digitHeight = SudokuGridSettings.Default.FontSize;
            float offsetY = -digitHeight / 2f;
            for (int x = 0; x < numGridColumns; x++)
            {
                for (int y = 0; y < numGridRows; y++)
                {
                    int gridNumber = (y * numGridColumns) + x;                    
                    float numberOfDigits = gridNumber.ToString().Length;                    
                    float offsetX = -digitWidth * numberOfDigits * 0.5f;                    
                    float positionX = x * cellWidth + cellWidth / 2f + offsetX;
                    float positionY = y * cellHeight + cellHeight / 2f + offsetY;

                    numberPositions[gridNumber] = new Point(positionX, positionY);                  

                    for (int i = 1; i < GridSize; i++)
                    {
                        float startX = x * cellWidth + i * cellWidth / 3;
                        float startY = y * cellHeight;
                        float endX = startX;
                        float endY = startY + cellHeight;
                        smallGridGeometry.BeginFigure(new Point(startX, startY));
                        smallGridGeometry.AddPoint(new Point(endX, endY));
                        smallGridGeometry.EndFigure();
                    }

                    for (int j = 1; j < GridSize; j++)
                    {
                        float startX = x * cellWidth;
                        float startY = y * cellHeight + j * cellHeight / 3;
                        float endX = startX + cellWidth;
                        float endY = startY;
                        smallGridGeometry.BeginFigure(new Point(startX, startY));
                        smallGridGeometry.AddPoint(new Point(endX, endY));
                        smallGridGeometry.EndFigure();
                    }
                }
            }
            smallGridGeometry.Close(); 
            
        }

        protected override void DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
        {
            base.DestroyGraphics(sender, e);
            gridNumberFont.Dispose();
            bigGridGeometry.Dispose();
            smallGridGeometry.Dispose();
            lineBrush.Dispose();
            secondaryLineBrush.Dispose();
            backgroundBrush.Dispose();
        }

        protected override void DrawGraphics(object sender, DrawGraphicsEventArgs e)
        {
            if (!refreshGrid) return;
            var g = e.Graphics;
            g.ClearScene();
            g.FillRectangle(backgroundBrush, 0, 0, screenWidth, screenHeight);            
            
            g.DrawGeometry(bigGridGeometry, lineBrush, SudokuGridSettings.Default.StrokeSize);
            g.DrawGeometry(smallGridGeometry, secondaryLineBrush, SudokuGridSettings.Default.SecondaryStrokeSize);
            
            for (int i = 0; i < numberPositions.Length; i++)            
                g.DrawText(gridNumberFont, textBrush, numberPositions[i].X, numberPositions[i].Y, i.ToString());
            refreshGrid = false;
        }
    }
}
