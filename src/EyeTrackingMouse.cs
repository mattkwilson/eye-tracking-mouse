using System.ComponentModel;
using System.Windows.Forms;
using EyeTrackingMouse.Properties;
using Tobii.StreamEngine;
using System;
using System.Diagnostics;
using EyeTrackingMouse.exceptions;
using EyeTrackingMouse.extensions;
using EyeTrackingMouse.graphics;
using System.Runtime.InteropServices;
using System.Threading;

namespace EyeTrackingMouse
{
    public enum HeadTrackingType
    {
        Both,
        Vertical,
        Horizontal
    };
    public enum InteractorId
    {
        None = -1,
        ScrollUp,
        ScrollDown,
        ScrollLeft,
        ScrollRight
    };
    class EyeTrackingMouse 
    {       
        private const string TobiiConfigurationPath = "c:\\Program Files (x86)\\Tobii\\Tobii Configuration\\Tobii.Configuration.exe";
        private const int AutosaveInterval = 60000; 
        private const int GazeJumpThreshold = 150;        
        private const int GazeJumpCorrectionCount = 3;
        private const int HeadTrackingRotationFactor = 100;
        private const float HeadTrackingInterpolationFactor = 0.8f;

        private DisplayHandler displayHandler;
        private SystemTrayIcon trayIcon;
        private GraphicalOverlay graphics;
        private InputHandler inputHandler;                
        private System.Timers.Timer autosaveTimer;

        private IntPtr apiContext;
        private IntPtr deviceContext;
        private Thread streamEngineThread;

        private tobii_gaze_point_callback_t gazePointCallback;
        private tobii_head_pose_callback_t headPoseCallback;

        private Vector2 gazePosition;
        public Vector2 GazePosition {
            get { return gazePosition; }
        }
        private Vector2 headPosition;
        private Vector2 headRotation;
        private HeadTrackingType headTrackingType;
        private Vector2 nextCursorPosition;
        private bool resetNextCursorPosition;
        public bool ResetNextCursorPosition {
            set { resetNextCursorPosition = value;  }
        }
        private InteractorId focusedInteractor;
        public InteractorId FocusedInteractor {
            get { return focusedInteractor; }
        }

        private int gazeJumpCounter;
        private bool terminated;

        public EyeTrackingMouse()
        {
            displayHandler = new DisplayHandler();
            displayHandler.MainDisplayUpdated += ReInitialize;

            graphics = new GraphicalOverlay(this, displayHandler.MainDisplay.Bounds);
            graphics.Create();

            inputHandler = new MyInputHandler(this, displayHandler, graphics);

            trayIcon = new SystemTrayIcon(displayHandler);
            trayIcon.ExitEvent += Exit;

            InitializeStreamEngine();

            autosaveTimer = new System.Timers.Timer();
            autosaveTimer.Interval = AutosaveInterval;
            autosaveTimer.Elapsed += Autosave;
            autosaveTimer.Enabled = true;                    
        }

        private void InitializeStreamEngine()
        {
            // create the API
            ProcessTobiiErrors(Interop.tobii_api_create(out apiContext, null));
            // find all the local tobii eye tracking devices
            ProcessTobiiErrors(Interop.tobii_enumerate_local_device_urls(apiContext, out var urls));
            if (urls.Count == 0)
                throw new TobiiStreamEngineException("No device found");
            // create the device instance
            ProcessTobiiErrors(Interop.tobii_device_create(apiContext, urls[0],
                Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, out deviceContext));

            gazePointCallback = OnGazePoint;
            headPoseCallback = OnHeadPose;
            // subscribe to events
            ProcessTobiiErrors(Interop.tobii_gaze_point_subscribe(deviceContext, gazePointCallback));
            ProcessTobiiErrors(Interop.tobii_head_pose_subscribe(deviceContext, headPoseCallback));

            streamEngineThread = new Thread(RunStreamEngine)
            {
                IsBackground = true
            };
            streamEngineThread.Start();
        }

        private void RunStreamEngine()
        {
            var devices = new[] { deviceContext };
            // process the callbacks
            while (!terminated)
            {
                ProcessTobiiErrors(Interop.tobii_wait_for_callbacks(devices), true);
                var result = Interop.tobii_device_process_callbacks(deviceContext);

                while (result == tobii_error_t.TOBII_ERROR_CONNECTION_FAILED)
                {
                    Thread.Sleep(200);
                    if (deviceContext == IntPtr.Zero) return;
                    result = Interop.tobii_device_reconnect(deviceContext);
                }
            }
        }

        private void ProcessTobiiErrors(tobii_error_t e, bool allowTimeout = false)
        {
            if (e == tobii_error_t.TOBII_ERROR_NO_ERROR || (allowTimeout && e == tobii_error_t.TOBII_ERROR_TIMED_OUT))
                return;
            Trace.WriteLine(e.ToString());
            throw new TobiiStreamEngineException(e);
        }

        public void ReInitialize()
        {
            SetInteractionDisplay();
            graphics.SetSize(displayHandler.MainDisplay.Bounds);            
        }

        public void SetInteractionDisplay()
        {            
            Process.Start(TobiiConfigurationPath, "-S");
        }

        public void RecalibrateEyeTracker()
        {            
            Process.Start(TobiiConfigurationPath, "-Q");
        }

        public void SetHeadTrackingType(HeadTrackingType type)
        {
            headTrackingType = type;
            resetNextCursorPosition = true;
        }
        
        private void OnHeadPose(ref tobii_head_pose_t headPose, IntPtr userData)
        {
            if (!Settings.Default.IsHeadTrackingEnabled)
                return;
            
            if (headPose.position_validity == tobii_validity_t.TOBII_VALIDITY_INVALID
               || headPose.rotation_x_validity == tobii_validity_t.TOBII_VALIDITY_INVALID
               || headPose.rotation_y_validity == tobii_validity_t.TOBII_VALIDITY_INVALID)
                return;

            float positionX = -headPose.position_xyz.x;
            float positionY = headPose.position_xyz.y;
            float rotationX = -headPose.rotation_xyz.y * HeadTrackingRotationFactor;
            float rotationY = headPose.rotation_xyz.x * HeadTrackingRotationFactor;

            if (headPosition.X == 0 && headPosition.Y == 0 && headRotation.X == 0 && headRotation.Y == 0)
            {
                headPosition = new Vector2(positionX, positionY);
                headRotation = new Vector2(rotationX, rotationY);
                resetNextCursorPosition = true;
                return;
            }

            if (resetNextCursorPosition)
            {
                nextCursorPosition = new Vector2(Cursor.Position.X, Cursor.Position.Y);
                resetNextCursorPosition = false;
            }
            
            double deltaX = (headPosition.X - positionX /*+ (headRotation.X - rotationX)*/) * Settings.Default.CursorSpeed;
            double deltaY = (/*headPosition.Y - positionY +*/ (headRotation.Y - rotationY)) * Settings.Default.CursorSpeed;
            
            switch (headTrackingType)
            {
                case HeadTrackingType.Both:
                    nextCursorPosition = new Vector2(nextCursorPosition.X + deltaX, nextCursorPosition.Y + deltaY);
                    Cursor.Position = Utility.Lerp(Cursor.Position, nextCursorPosition, HeadTrackingInterpolationFactor);
                    break;
                case HeadTrackingType.Vertical:
                    nextCursorPosition = new Vector2(nextCursorPosition.X, nextCursorPosition.Y + deltaY);
                    Cursor.Position = Utility.Lerp(Cursor.Position, nextCursorPosition, HeadTrackingInterpolationFactor);
                    break;
                case HeadTrackingType.Horizontal:
                    nextCursorPosition = new Vector2(nextCursorPosition.X + deltaX, nextCursorPosition.Y);
                    Cursor.Position = Utility.Lerp(Cursor.Position, nextCursorPosition, HeadTrackingInterpolationFactor);
                    break;
            }

            headPosition = new Vector2(positionX, positionY);
            headRotation = new Vector2(rotationX, rotationY);
        }

        private void OnGazePoint(ref tobii_gaze_point_t gazePoint, IntPtr userData)
        {
            if (gazePoint.validity == tobii_validity_t.TOBII_VALIDITY_INVALID)
                return;
            const float tolerance = 0.1f;
            int displayWidth = displayHandler.MainDisplay.Bounds.Width;
            int displayHeight = displayHandler.MainDisplay.Bounds.Height;
            Vector2 newGazePoint = new Vector2(gazePoint.position.x * displayWidth,
                gazePoint.position.y * displayHeight);
            // The gaze position is outside the bounds of the screen
            if (Math.Abs(newGazePoint.X - (-displayWidth)) < tolerance 
                && Math.Abs(newGazePoint.Y - (-displayHeight)) < tolerance)                               
                return;
            
            double distance = Utility.Distance(gazePosition, newGazePoint);
            // Allow the gaze position to jump to the next position if the distance
            // passes a certain threshold
            if (distance > GazeJumpThreshold)
            {
                // Wait a few frames to reduce jitter
                gazeJumpCounter++;
                if (gazeJumpCounter >= GazeJumpCorrectionCount)
                {
                    gazePosition = newGazePoint;
                    gazeJumpCounter = 0;
                }                
            }
            else
                gazePosition = Utility.Lerp(gazePosition, newGazePoint, distance / GazeJumpThreshold);
            graphics.SetCrosshairPosition((float)gazePosition.X, (float)gazePosition.Y);
            
            HandleInteractors(newGazePoint);
        }

        private void HandleInteractors(Vector2 gazePoint)
        {
            var prevFocusedInteractor = focusedInteractor;
            if (graphics.ScrollUpRectangle.Contains(gazePoint))
            {
                focusedInteractor = InteractorId.ScrollUp;
            } else if (graphics.ScrollDownRectangle.Contains(gazePoint))
            {
                focusedInteractor = InteractorId.ScrollDown;
            } else if (graphics.ScrollLeftRectangle.Contains(gazePoint))
            {
                focusedInteractor = InteractorId.ScrollLeft;
            } else if (graphics.ScrollRightRectangle.Contains(gazePoint))
            {
                focusedInteractor = InteractorId.ScrollRight;
            } else
            {
                focusedInteractor = InteractorId.None;
            }
            
            if (prevFocusedInteractor != focusedInteractor)
            {
                inputHandler.SetScrollType((ScrollType)focusedInteractor);
            }
        }
      
        private void Autosave(object sender, EventArgs e) 
        {
            SudokuGridSettings.Default.Save();
            OverlaySettings.Default.Save();
            Settings.Default.Save();
        }
        
        public void Exit()
        {
            terminated = true;
            displayHandler.Dispose();
            inputHandler.Dispose();
            trayIcon.Dispose();
            graphics.Dispose();

            streamEngineThread.Join(500);

            ProcessTobiiErrors(Interop.tobii_gaze_point_unsubscribe(deviceContext));
            ProcessTobiiErrors(Interop.tobii_head_pose_unsubscribe(deviceContext));
            ProcessTobiiErrors(Interop.tobii_device_destroy(deviceContext));
            ProcessTobiiErrors(Interop.tobii_api_destroy(apiContext));

            SudokuGridSettings.Default.Save();
            OverlaySettings.Default.Save();
            Settings.Default.Save();
            Application.Exit();
        }

        
    }
}
