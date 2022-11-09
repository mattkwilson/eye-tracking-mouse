using System;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using WindowsInput;
using System.ComponentModel;
using System.Timers;
using Windows.Foundation;
using WindowsInput.Native;
using EyeTrackingMouse.Properties;
using CMatchOCR;
using EyeTrackingMouse.graphics;
using Point = System.Drawing.Point;

namespace EyeTrackingMouse
{
    public enum Direction
    {
        UpLeft = 1,
        Up,
        UpRight,
        Left,
        None,
        Right,
        DownLeft,
        Down,
        DownRight
    };

    public enum ScrollType
    {
        None = -1,
        Up,
        Down,
        Left,
        Right
    };

    public enum WordSnapOption
    {
        Snap,
        Click,
        Select,
        GoFront,
        GoEnd
    };

    class InputHandler
    {
        public const int ScanRate = 100;
        public const float ContinuousFollowUpdateRate = 0.01f;
        public const string ServerAddress = "http://localhost:8000/EyeTracker/";

        protected InputSimulator inputSimulator;
        protected IKeyboardMouseEvents inputEvents;
        protected EyeTrackingMouse trackingMouse;
        protected GraphicalOverlay graphicalOverlay;
        protected DisplayHandler displayHandler;
        protected HttpListener httpListener;
        protected Stopwatch snapStopwatch;
        protected System.Timers.Timer scrollTimer;
        protected System.Timers.Timer continuousFollowTimer;
        protected ControlSnapper controlSnapper;
        protected ScreenTextFinder wordFinder;

        protected ScrollType scrollType;

        protected double gridCellWidth;
        protected double gridCellHeight;
        protected double secondaryGridCellWidth;
        protected double secondaryGridCellHeight;
        protected Vector2[,] gridPositions;
        protected Vector2[] gridNumberPositions;

        private BackgroundWorker serverWorker;
        private bool terminated;

        public InputHandler(EyeTrackingMouse trackingMouse, DisplayHandler displayHandler,
            GraphicalOverlay graphicalOverlay)
        {
            this.trackingMouse = trackingMouse;
            this.displayHandler = displayHandler;
            this.graphicalOverlay = graphicalOverlay;

            inputSimulator = new InputSimulator();

            inputEvents = Hook.GlobalEvents();
            inputEvents.KeyDown += KeyDown;
            inputEvents.KeyUp += KeyUp;
            inputEvents.MouseMove += MouseMove;

            httpListener = new HttpListener();
            httpListener.Prefixes.Add(ServerAddress);
            httpListener.Start();

            controlSnapper = new ControlSnapper();
            snapStopwatch = new Stopwatch();
            continuousFollowTimer = new System.Timers.Timer();
            continuousFollowTimer.Interval = ContinuousFollowUpdateRate;
            continuousFollowTimer.Elapsed += ContinuousFollowUpdate; 
            scrollTimer = new System.Timers.Timer();
            scrollTimer.Interval = Settings.Default.VerticalScrollRate;
            scrollTimer.Elapsed += Scroll;

            serverWorker = new BackgroundWorker();
            serverWorker.WorkerSupportsCancellation = true;
            serverWorker.DoWork += ServerLoop;
            serverWorker.RunWorkerAsync();

            displayHandler.MainDisplayUpdated += DisplayHandler_MainDisplayUpdated;
            SetGridParameters();

            wordFinder = new ScreenTextFinder();
        }

        private void DisplayHandler_MainDisplayUpdated()
        {
            SetGridParameters();
        }

        public void SetGridParameters()
        {
            const int size = 3;
            gridNumberPositions =
                new Vector2[SudokuGridSettings.Default.NumberOfColumns * SudokuGridSettings.Default.NumberOfRows];
            gridPositions = new Vector2[SudokuGridSettings.Default.NumberOfColumns * size,
                SudokuGridSettings.Default.NumberOfRows * size];
            gridCellWidth = displayHandler.MainDisplay.Bounds.Width /
                            (double) SudokuGridSettings.Default.NumberOfColumns;
            gridCellHeight = displayHandler.MainDisplay.Bounds.Height /
                             (double) SudokuGridSettings.Default.NumberOfRows;
            secondaryGridCellWidth = gridCellWidth / size;
            secondaryGridCellHeight = gridCellHeight / size;
            for (int x = 0; x < SudokuGridSettings.Default.NumberOfColumns; x++)
            {
                for (int y = 0; y < SudokuGridSettings.Default.NumberOfRows; y++)
                {
                    int gridNumber = (y * SudokuGridSettings.Default.NumberOfColumns) + x;

                    double positionX = x * gridCellWidth + gridCellWidth / 2;
                    double positionY = y * gridCellHeight + gridCellHeight / 2;

                    gridNumberPositions[gridNumber] = new Vector2(positionX, positionY);

                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            positionX = x * gridCellWidth + secondaryGridCellWidth * i + secondaryGridCellWidth / 2;
                            positionY = y * gridCellHeight + secondaryGridCellHeight * j + secondaryGridCellHeight / 2;
                            gridPositions[x * size + i, y * size + j] = new Vector2(positionX, positionY);
                        }
                    }
                }
            }
        }

        private void ServerLoop(object sender, DoWorkEventArgs e)
        {
            while (!terminated)
            {
                var context = httpListener.GetContext();
                var service = new EyeTrackingService(trackingMouse, this, graphicalOverlay);
                service.ProcessRequest(context);
            }
        }

        private void Scroll(object sender, EventArgs e)
        {
            switch (scrollType)
            {
                case ScrollType.Up:
                    inputSimulator.Mouse.VerticalScroll(1);
                    break;
                case ScrollType.Down:
                    inputSimulator.Mouse.VerticalScroll(-1);
                    break;
                case ScrollType.Left:
                    inputSimulator.Mouse.HorizontalScroll(-1);
                    break;
                case ScrollType.Right:
                    inputSimulator.Mouse.HorizontalScroll(1);
                    break;
            }
        }

        public void SetScrolling(bool isEnabled)
        {
            Settings.Default.IsScrollingEnabled = isEnabled;
            UpdateScrollTimer();
        }

        public void SetScrollType(ScrollType scrollType)
        {
            this.scrollType = scrollType;
            UpdateScrollTimer();
        }

        private void UpdateScrollTimer()
        {
            switch (scrollType)
            {
                case ScrollType.Up:
                case ScrollType.Down:
                    scrollTimer.Interval = Settings.Default.VerticalScrollRate;
                    break;
                case ScrollType.Left:
                case ScrollType.Right:
                    scrollTimer.Interval = Settings.Default.HorizontalScrollRate;
                    break;
            }

            scrollTimer.Enabled = Settings.Default.IsScrollingEnabled && scrollType != ScrollType.None;
        }

        public void SetScrollRate(int scrollRate, bool isHorizontal)
        {
            if (isHorizontal)
                Settings.Default.HorizontalScrollRate = scrollRate;
            else
                Settings.Default.VerticalScrollRate = scrollRate;
            UpdateScrollTimer();
        }

        public void SetContinuousFollow(bool isEnabled)
        {
            continuousFollowTimer.Enabled = isEnabled;
        }

        private void ContinuousFollowUpdate(object sender, ElapsedEventArgs e)
        {
            Vector2 point = new Vector2(trackingMouse.GazePosition.X, trackingMouse.GazePosition.Y);
            point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
        }

        public void SnapToWord(string word, WordSnapOption option = WordSnapOption.Snap)
        {
            if (wordFinder.TryFindClosest(word, trackingMouse.GazePosition.X, trackingMouse.GazePosition.Y,
                200, out var rect))
            {
                switch (option)
                {
                    case WordSnapOption.Snap:
                        Vector2 point = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                        point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
                        inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
                        break;
                    case WordSnapOption.Click:
                        point = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                        point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
                        inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
                        inputSimulator.Mouse.LeftButtonClick();
                        break;
                    case WordSnapOption.Select:
                        point = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                        point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
                        inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
                        inputSimulator.Mouse.LeftButtonDoubleClick();
                        break;
                    case WordSnapOption.GoFront:
                        point = new Vector2(rect.X, rect.Y + rect.Height / 2f);
                        point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
                        inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
                        inputSimulator.Mouse.LeftButtonClick();
                        // inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        // inputSimulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
                        // inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        break;
                    case WordSnapOption.GoEnd:
                        point = new Vector2(rect.X + rect.Width, rect.Y + rect.Height / 2f);
                        point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
                        inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
                        inputSimulator.Mouse.LeftButtonClick();
                        // inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                        // inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                        // inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
                        break;
                }
            }
        }

        public void SnapCursor(int monitorIndex, bool click = false)
        {
            Vector2 point = new Vector2(trackingMouse.GazePosition.X, trackingMouse.GazePosition.Y);
            Screen currentScreen = Screen.AllScreens[monitorIndex];

            if (Settings.Default.IsSnapEnabled)
            {
                if (!snapStopwatch.IsRunning)
                {
                    SnapToNearestControl(currentScreen, point, click);
                    snapStopwatch.Start();
                }
                else if (snapStopwatch.ElapsedMilliseconds >= ScanRate)
                {
                    SnapToNearestControl(currentScreen, point, click);
                    snapStopwatch.Restart();
                }
            }
            else
            {
                point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, currentScreen, point);
                inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
                if (click)
                    inputSimulator.Mouse.LeftButtonClick();
            }
        }

        public void SnapToGrid(int gridNumber, Direction direction = Direction.None, int subGridNumber = -1,
            bool forceNoHide = false)
        {
            double x = 0;
            double y = 0;
            if (gridNumber == -1)
            {
                if (!displayHandler.MainDisplay.Equals(Screen.FromPoint(Cursor.Position)))
                    return;

                Point cursorPosition = Utility.GetRelativeCursorPosition(displayHandler.MainDisplay, Cursor.Position);
                int indexX = (int) (cursorPosition.X / secondaryGridCellWidth);
                int indexY = (int) (cursorPosition.Y / secondaryGridCellHeight);

                x = gridPositions[indexX, indexY].X;
                y = gridPositions[indexX, indexY].Y;
            }
            else
            {
                if (gridNumber >= gridNumberPositions.Length)
                    return;

                Vector2 gridNumberPosition = gridNumberPositions[gridNumber];
                x = gridNumberPosition.X;
                y = gridNumberPosition.Y;
            }

            switch (subGridNumber)
            {
                case 1:
                    x -= secondaryGridCellWidth / 4;
                    y -= secondaryGridCellHeight / 4;
                    break;
                case 2:
                    x += secondaryGridCellWidth / 4;
                    y -= secondaryGridCellHeight / 4;
                    break;
                case 3:
                    x -= secondaryGridCellWidth / 4;
                    y += secondaryGridCellHeight / 4;
                    break;
                case 4:
                    x += secondaryGridCellWidth / 4;
                    y += secondaryGridCellHeight / 4;
                    break;
            }

            Vector2 point = TranslatePointOnGrid(x, y, direction, 1);
            point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);

            if (!forceNoHide && OverlaySettings.Default.AutoHideGrid)
                graphicalOverlay.HideSudoku();
        }

        private Vector2 TranslatePointOnGrid(double x, double y, Direction direction, int amount)
        {
            switch (direction)
            {
                case Direction.UpLeft:
                    x -= secondaryGridCellWidth * amount;
                    y -= secondaryGridCellHeight * amount;
                    break;
                case Direction.Up:
                    //x += secondaryGridCellWidth * amount;
                    y -= secondaryGridCellHeight * amount;
                    break;
                case Direction.UpRight:
                    x += secondaryGridCellWidth * amount;
                    y -= secondaryGridCellHeight * amount;
                    break;
                case Direction.Left:
                    x -= secondaryGridCellWidth * amount;
                    //y += secondaryGridCellHeight * amount;
                    break;
                case Direction.None:
                    //x -= secondaryGridCellWidth * amount;
                    //y += secondaryGridCellHeight * amount;
                    break;
                case Direction.Right:
                    x += secondaryGridCellWidth * amount;
                    //y -= secondaryGridCellHeight * amount;
                    break;
                case Direction.DownLeft:
                    x -= secondaryGridCellWidth * amount;
                    y += secondaryGridCellHeight * amount;
                    break;
                case Direction.Down:
                    //x += secondaryGridCellWidth * amount;
                    y += secondaryGridCellHeight * amount;
                    break;
                case Direction.DownRight:
                    x += secondaryGridCellWidth * amount;
                    y += secondaryGridCellHeight * amount;
                    break;
            }

            return new Vector2(x, y);
        }

        public void MoveAlongGrid(Direction direction, int amount, bool forceNoHide)
        {
            Point cursorPosition = Utility.GetRelativeCursorPosition(displayHandler.MainDisplay, Cursor.Position);
            int x = (int) (cursorPosition.X / secondaryGridCellWidth);
            int y = (int) (cursorPosition.Y / secondaryGridCellHeight);

            switch (direction)
            {
                case Direction.UpLeft:
                    x -= amount;
                    y -= amount;
                    break;
                case Direction.Up:
                    //x += amount;
                    y -= amount;
                    break;
                case Direction.UpRight:
                    x += amount;
                    y -= amount;
                    break;
                case Direction.Left:
                    x -= amount;
                    //y += amount;
                    break;
                case Direction.None:
                    //x -= amount;
                    //y += amount;
                    break;
                case Direction.Right:
                    x += amount;
                    //y -= amount;
                    break;
                case Direction.DownLeft:
                    x -= amount;
                    y += amount;
                    break;
                case Direction.Down:
                    //x += amount;
                    y += amount;
                    break;
                case Direction.DownRight:
                    x += amount;
                    y += amount;
                    break;
            }

            int maxX = gridPositions.GetLength(0);
            int maxY = gridPositions.GetLength(1);
            x = x < 0 ? 0 : x >= maxX ? maxX - 1 : x;
            y = y < 0 ? 0 : y >= maxY ? maxY - 1 : y;

            Vector2 point = gridPositions[x, y];
            point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay, point);
            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);

            if (!forceNoHide && OverlaySettings.Default.AutoHideGrid)
                graphicalOverlay.HideSudoku();
        }

        public virtual void Dispose()
        {
            inputEvents.Dispose();
            scrollTimer.Dispose();
            serverWorker.CancelAsync();
            serverWorker.Dispose();

            terminated = true;
        }

        protected void SnapToNearestControl(Screen screen, Vector2 point, bool click = false)
        {
            point = Utility.TransformScreenPosition(displayHandler.MainDisplay, screen, point);
            controlSnapper.GetNearestClickableControlpoint(ref point);
            point = Utility.TransformVirtualScreenPosition(screen, point);
            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);

            if (click)
                inputSimulator.Mouse.LeftButtonClick();
        }

        protected virtual void MouseMove(object sender, MouseEventArgs e)
        {
            trackingMouse.ResetNextCursorPosition = true;
        }

        protected virtual void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Vector2 point = Utility.TransformVirtualScreenPosition(displayHandler.MainDisplay,
                    displayHandler.MainDisplay, trackingMouse.GazePosition);
                inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(point.X, point.Y);
            }
        }

        protected virtual void KeyUp(object sender, KeyEventArgs e)
        {
        }
    }
}