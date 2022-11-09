using System;
using System.Drawing;
using System.Windows.Automation;
using EyeTrackingMouse.graphics;

namespace EyeTrackingMouse
{
    class ControlSnapper
    {
        public const int ScanDensity = 15;

        public ControlSnapper() 
        { 
        
        }

        #region ControlSnapping

        public bool GetNearestClickableControlpoint(ref Vector2 point)
        {
            // Attempt to try and find a nearby clickable control element by
            // searching for Automation Elements at coordinates surrounding the
            // given point.            
            try
            {
                System.Windows.Point tempPoint = new System.Windows.Point(point.X, point.Y);
                AutomationElement element = AutomationElement.FromPoint(tempPoint);
                System.Windows.Point clickablePoint;
                if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                {
                    point = new Vector2(clickablePoint.X, clickablePoint.Y);
                    return true;
                }
                else
                {
                    int count = ScanDensity;

                    while (count <= GraphicalOverlay.CrosshairRadius)
                    {
                        //Left
                        tempPoint = new System.Windows.Point(point.X - count, point.Y);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //Right
                        tempPoint = new System.Windows.Point(point.X + count, point.Y);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //up
                        tempPoint = new System.Windows.Point(point.X, point.Y + count);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //down
                        tempPoint = new System.Windows.Point(point.X, point.Y - count);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //Up Left
                        tempPoint = new System.Windows.Point(point.X - count, point.Y + count);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //Up Right
                        tempPoint = new System.Windows.Point(point.X + count, point.Y + count);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //Down left
                        tempPoint = new System.Windows.Point(point.X - count, point.Y - count);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        //Down Right
                        tempPoint = new System.Windows.Point(point.X + count, point.Y - count);
                        element = AutomationElement.FromPoint(tempPoint);
                        if (IsValidControl(element) && element.TryGetClickablePoint(out clickablePoint))
                        {
                            point = new Vector2(clickablePoint.X, clickablePoint.Y);
                            return true;
                        }
                        count += ScanDensity;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public bool IsValidControl(AutomationElement element)
        {
            return element.Current.ControlType == ControlType.Button ||
                    element.Current.ControlType == ControlType.CheckBox ||
                    element.Current.ControlType == ControlType.ComboBox ||
                    element.Current.ControlType == ControlType.DataItem ||
                    element.Current.ControlType == ControlType.Custom ||
                    element.Current.ControlType == ControlType.Hyperlink ||
                    element.Current.ControlType == ControlType.ListItem ||
                    element.Current.ControlType == ControlType.MenuItem ||
                    element.Current.ControlType == ControlType.RadioButton ||
                    element.Current.ControlType == ControlType.SplitButton ||
                    element.Current.ControlType == ControlType.TabItem;
        }

        #endregion
    }
}
