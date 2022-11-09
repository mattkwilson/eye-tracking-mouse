using EyeTrackingMouse.Properties;
using CookComputing.XmlRpc;
using System;
using EyeTrackingMouse.graphics;

namespace EyeTrackingMouse
{    
    class EyeTrackingService : XmlRpcListenerService
    {
        private EyeTrackingMouse eyeTrackingMouse;
        private InputHandler inputHandler;
        private GraphicalOverlay graphicalOverlay;
        private SudokuGrid sudokuGrid;
        public EyeTrackingService(EyeTrackingMouse eyeTrackingMouse, InputHandler inputHandler, 
        GraphicalOverlay graphicalOverlay) : base()
        {
            this.eyeTrackingMouse = eyeTrackingMouse;
            this.inputHandler = inputHandler;
            this.graphicalOverlay = graphicalOverlay;
            sudokuGrid = graphicalOverlay.GetSudokuGrid();
        }
        #region Main Controls
        [XmlRpcMethod("EyeTracking.SnapAndClick")]
        public void SnapAndClick(int monitorIndex = -1)
        {
            if(monitorIndex == -1)
                inputHandler.SnapCursor(Settings.Default.MainDisplayIndex, true);
            else
                inputHandler.SnapCursor(monitorIndex, true);
        }
        [XmlRpcMethod("EyeTracking.SnapCursor")]
        public void SnapCursor(int monitorIndex = -1)
        {
            if (monitorIndex == -1)
                inputHandler.SnapCursor(Settings.Default.MainDisplayIndex);
            else
                inputHandler.SnapCursor(monitorIndex);
        }

        [XmlRpcMethod("EyeTracking.ContinuousFollow")]
        public void ContinuousFollow(bool isEnabled)
        {
            inputHandler.SetContinuousFollow(isEnabled);   
        }
        [XmlRpcMethod("EyeTracking.SnapToWord")]
        public void SnapToWord(string word, WordSnapOption option)
        {
            inputHandler.SnapToWord(word, option);
        }
        [XmlRpcMethod("EyeTracking.SetScrolling")]
        public void SetScrolling(bool isEnabled)
        {
            inputHandler.SetScrolling(isEnabled);
        }
        [XmlRpcMethod("EyeTracking.SetScrollRate")]
        public void SetScrollRate(int scrollRate, bool isHorizontal)
        {            
            inputHandler.SetScrollRate(scrollRate, isHorizontal);
        }
        [XmlRpcMethod("EyeTracking.SetScrollInteractorPosition")]
        public void SetScrollInteractorPosition(int scrollInteractorPosition)
        {
            OverlaySettings.Default.VerticalScrollIndicatorPosition = scrollInteractorPosition;
            graphicalOverlay.Refresh();
        }
        
        [XmlRpcMethod("EyeTracking.SetCursorSnapping")]
        public void SetCursorSnapping(bool isEnabled)
        {
            Settings.Default.IsSnapEnabled = isEnabled;
        }
        [XmlRpcMethod("EyeTracking.SetHeadTracking")]
        public void SetHeadTracking(HeadTrackingType type)
        {
            Settings.Default.IsHeadTrackingEnabled = true;
            eyeTrackingMouse.SetHeadTrackingType(type);
        }
        [XmlRpcMethod("EyeTracking.DisableHeadTracking")]
        public void DisableHeadTracking()
        {
            Settings.Default.IsHeadTrackingEnabled = false;
        }
        [XmlRpcMethod("EyeTracking.SetMainDisplay")]
        public void SetMainDisplay(int displayIndex)
        {
            Settings.Default.MainDisplayIndex = displayIndex;
        }
        [XmlRpcMethod("EyeTracking.SetCursorSpeed")]
        public void SetCursorSpeed(int speed)
        {
            Settings.Default.CursorSpeed = speed;
        }
        [XmlRpcMethod("EyeTracking.Exit")]
        public void Exit()
        {
            eyeTrackingMouse.Exit();
        }
        #endregion

        #region Tobii
        [XmlRpcMethod("EyeTracking.SetEyeTrackerDisplay")]
        public void SetEyeTrackerDisplay()
        {
            eyeTrackingMouse.SetInteractionDisplay();
        }
        [XmlRpcMethod("EyeTracking.Recalibrate")]
        public void Recalibrate()
        {
            eyeTrackingMouse.RecalibrateEyeTracker();
        }
        #endregion

        #region Overlay
        [XmlRpcMethod("EyeTracking.SetCrosshairType")]
        public void SetCrosshairType(bool isSimple)
        {
            OverlaySettings.Default.IsSimpleCrosshair = isSimple;
        }
        [XmlRpcMethod("EyeTracking.SetCrosshairVisibility")]
        public void SetCrosshairVisibility(bool isVisible)
        {
            OverlaySettings.Default.IsCrosshairVisible = isVisible;
        }
        #endregion 

        #region Sudoku Controls
        [XmlRpcMethod("EyeTracking.SetGrid")]
        public void SetGrid(bool isVisible)
        {
            if (isVisible)
                graphicalOverlay.ShowSudoku();
            else
                graphicalOverlay.HideSudoku();
        }
        [XmlRpcMethod("EyeTracking.AutoHideGrid")]
        public void AutoHideGrid()
        {
            OverlaySettings.Default.AutoHideGrid = !OverlaySettings.Default.AutoHideGrid;
        }
        [XmlRpcMethod("EyeTracking.SnapToGrid")]
        public void SnapToGrid(int gridNumber, Direction direction, int subGridNumber, bool forceNoHide)
        {
            inputHandler.SnapToGrid(gridNumber, direction, subGridNumber, forceNoHide);
        }
        [XmlRpcMethod("EyeTracking.MoveAlongGrid")]
        public void MoveAlongGrid(Direction direction, int amount, bool forceNoHide)
        {
            inputHandler.MoveAlongGrid(direction, amount, forceNoHide);
        }
        #endregion 

        #region Sudoku Settings
        [XmlRpcMethod("EyeTracking.Sudoku.SetStrokeSize")]
        public void SetStrokeSize(double strokeSize)
        {
            SudokuGridSettings.Default.StrokeSize = (float)strokeSize;
            sudokuGrid.Refresh();            
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetSecondaryStrokeSize")]
        public void SetSecondaryStrokeSize(double secondaryStrokeSize)
        {
            SudokuGridSettings.Default.SecondaryStrokeSize = (float)secondaryStrokeSize;
            sudokuGrid.Refresh();            
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetFontSize")]
        public void SetFontSize(int fontSize)
        {
            SudokuGridSettings.Default.FontSize = fontSize;
            sudokuGrid.Refresh();          
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetNumberOfColumns")]
        public void SetNumberOfColumns(int numberOfColumns)
        {
            SudokuGridSettings.Default.NumberOfColumns = numberOfColumns;
            sudokuGrid.Refresh();
            inputHandler.SetGridParameters();
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetNumberOfRows")]
        public void SetNumberOfRows(int numberOfRows)
        {
            SudokuGridSettings.Default.NumberOfRows = numberOfRows;
            sudokuGrid.Refresh();
            inputHandler.SetGridParameters();
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetBackgroundOpacity")]
        public void BackgroundOpacity(double backgroundOpacity)
        {
            SudokuGridSettings.Default.BackgroundOpacity = (float)backgroundOpacity;
            sudokuGrid.Refresh();
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetTextOpacity")]
        public void SetTextOpacity(double textOpacity)
        {
            SudokuGridSettings.Default.TextOpacity = (float)textOpacity;
            sudokuGrid.Refresh();
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetStrokeOpacity")]
        public void SetStrokeOpacity(double strokeOpacity)
        {
            SudokuGridSettings.Default.StrokeOpacity = (float)strokeOpacity;
            sudokuGrid.Refresh();
        }
        [XmlRpcMethod("EyeTracking.Sudoku.SetSecondaryStrokeOpacity")]
        public void SetSecondaryStrokeOpacity(double secondaryStrokeOpacity)
        {
            SudokuGridSettings.Default.SecondaryStrokeOpacity = (float)secondaryStrokeOpacity;
            sudokuGrid.Refresh();
        }
        #endregion

    }
}
