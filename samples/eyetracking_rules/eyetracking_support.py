import xmlrpc.client as xc
import ctypes
import subprocess

server_url = 'http://localhost:8000/EyeTracker'
server = xc.Server(server_url)


def set_cursor_speed(cursor_speed):    
    set_mouse_speed = 113   # 0x0071 for SPI_SETMOUSESPEED
    ctypes.windll.user32.SystemParametersInfoA(set_mouse_speed, 0, cursor_speed, 0)
    server.EyeTracking.SetCursorSpeed(cursor_speed)

def get_cursor_speed():
    get_mouse_speed = 112   # 0x0070 for SPI_GETMOUSESPEED
    speed = ctypes.c_int()
    ctypes.windll.user32.SystemParametersInfoA(get_mouse_speed, 0, ctypes.byref(speed), 0)
    return speed.value
    
def launch_eye_tracking_app():
    subprocess.Popen('C:\\Users\\sk8te\\Documents\\Github\\EyeTrackingMouse\\bin\\x64\\Release\\EyeTrackingMouse.exe')    

def snapper(monitor_index=-1):    
    server.EyeTracking.SnapAndClick(monitor_index)
    
def snap(monitor_index=-1):
    server.EyeTracking.SnapCursor(monitor_index)
    
def continuous_follow(is_enabled):
    server.EyeTracking.ContinuousFollow(is_enabled)

def snap_to_word(text, word_snap_option):
    server.EyeTracking.SnapToWord(str(text), word_snap_option)

def scroll(is_enabled):
    server.EyeTracking.SetScrolling(is_enabled)

def scroll_rate(time_in_milliseconds, is_horizontal=False):
    server.EyeTracking.SetScrollRate(time_in_milliseconds, is_horizontal)

def scroll_rate_exact(n, is_horizontal=False):
    server.EyeTracking.SetScrollRate(n, is_horizontal)

def set_scroll_interactor_position(scrolling_interactor_position):
    server.EyeTracking.SetScrollInteractorPosition(scrolling_interactor_position)

def set_tobii_display(): 
    server.EyeTracking.SetEyeTrackerDisplay()

def recalibrate():
    server.EyeTracking.Recalibrate()

def set_crosshair_simple(is_simple):
    server.EyeTracking.SetCrosshairType(is_simple)

def set_crosshair_visibility(is_visible):
    server.EyeTracking.SetCrosshairVisibility(is_visible)

def set_cursor_snapping(is_enabled):
    server.EyeTracking.SetCursorSnapping(is_enabled)

def set_head_tracking(head_tracking_type):
    server.EyeTracking.SetHeadTracking(head_tracking_type)

def disable_head_tracking():
    server.EyeTracking.DisableHeadTracking()

def set_display(monitor_number):
    server.EyeTracking.SetMainDisplay(monitor_number - 1)

def close_eye_tracker():
    server.EyeTracking.Exit()

# sudoku
def set_grid(is_visible):
    server.EyeTracking.SetGrid(is_visible)

def auto_hide_grid():
    server.EyeTracking.AutoHideGrid()

def snap_to_grid(n=-1, direction=5, nnn=-1, force_no_hide=False):
    server.EyeTracking.SnapToGrid(n, direction, nnn, force_no_hide)

def move_along_grid(direction, n=1, force_no_hide=False):
    server.EyeTracking.MoveAlongGrid(direction, n, force_no_hide)

# sudoku settings
def set_stroke_size(stroke_size):
    server.EyeTracking.Sudoku.SetStrokeSize(stroke_size / 100.0)

def set_secondary_stroke_size(stroke_size):
    server.EyeTracking.Sudoku.SetSecondaryStrokeSize(stroke_size / 100.0)

def set_font_size(font_size):
    server.EyeTracking.Sudoku.SetFontSize(font_size)

def set_number_of_columns(column_number):
    server.EyeTracking.Sudoku.SetNumberOfColumns(column_number)
    
def set_number_of_rows(row_number):
    server.EyeTracking.Sudoku.SetNumberOfRows(row_number)

def set_background_opacity(opacity):
    server.EyeTracking.Sudoku.SetBackgroundOpacity(opacity / 100.0)

def set_text_opacity(opacity):
    server.EyeTracking.Sudoku.SetTextOpacity(opacity / 100.0)

def set_stroke_opacity(opacity):
    server.EyeTracking.Sudoku.SetStrokeOpacity(opacity / 100.0)

def set_secondary_stroke_opacity(opacity):
    server.EyeTracking.Sudoku.SetSecondaryStrokeOpacity(opacity / 100.0)
