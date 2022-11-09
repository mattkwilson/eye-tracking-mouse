from dragonfly import (Choice, Function, MappingRule, Dictation)
from castervoice.lib.merge.additions import IntegerRefST
from castervoice.lib.ctrl.mgr.rule_details import RuleDetails
from castervoice.lib.merge.state.short import R

from eyetracking_rules import eyetracking_support as etc

class EyeTracking(MappingRule):
    mapping = {        
    "snapper [<monitor_index>]": R(Function(etc.snapper)),   
    "snap [<monitor_index>]": R(Function(etc.snap)),
    "enable [continuous] follow[ing]": R(Function(etc.continuous_follow, is_enabled=True)),
    "disable [continuous] follow[ing]": R(Function(etc.continuous_follow, is_enabled=False)),
    # words snapping
    "<word_snap_option> <text>": R(Function(etc.snap_to_word)),    
        
    "enable scrolling|scrolling on": R(Function(etc.scroll, is_enabled=True)),
    "disable scrolling|scrolling off": R(Function(etc.scroll, is_enabled=False)),    
    "[<is_horizontal>] scroll rate <time_in_milliseconds>": R(Function(etc.scroll_rate)),
    "[<is_horizontal>] scroll rate <n>": R(Function(etc.scroll_rate_exact)),
    "set scroll[ing] [interactor] <scrolling_interactor_position>": R(Function(etc.set_scroll_interactor_position)),
    # sudoku   
    "show grid": R(Function(etc.set_grid, is_visible=True)),
    "hide grid": R(Function(etc.set_grid, is_visible=False)),
    "[toggle] auto hide grid": R(Function(etc.auto_hide_grid)),    
    "grid sub <nnn>": R(Function(etc.snap_to_grid)),    
    "grid sub <nnn> show": R(Function(etc.snap_to_grid, force_no_hide=True)),    
    "grid <n> [<direction>] [sub <nnn>]": R(Function(etc.snap_to_grid)),    
    "grid <n> [<direction>] [sub <nnn>] show": R(Function(etc.snap_to_grid, force_no_hide=True)),    
    "grid <direction> [<n>]": R(Function(etc.move_along_grid)),
    "grid <direction> [<n>] show": R(Function(etc.move_along_grid, force_no_hide=True)),
    # settings
    "grid stroke size <stroke_size>": R(Function(etc.set_stroke_size)),
    "grid secondary stroke size <stroke_size>": R(Function(etc.set_secondary_stroke_size)),
    "grid font size <font_size>": R(Function(etc.set_font_size)),
    "grid number [of] columns <column_number>": R(Function(etc.set_number_of_columns)),
    "grid number [of] rows <row_number>": R(Function(etc.set_number_of_rows)),
    "grid background opacity <opacity>": R(Function(etc.set_background_opacity)),
    "grid text opacity <opacity>": R(Function(etc.set_text_opacity)),
    "grid stroke opacity <opacity>": R(Function(etc.set_stroke_opacity)),
    "grid secondary stroke opacity <opacity>": R(Function(etc.set_secondary_stroke_opacity)),
    # ---
    "set interaction display": R(Function(etc.set_tobii_display)),   
    "recalibrate": R(Function(etc.recalibrate)),   
    "set crosshair simple": R(Function(etc.set_crosshair_simple, is_simple=True)),   
    "set crosshair default": R(Function(etc.set_crosshair_simple, is_simple=False)),   
    "show crosshair": R(Function(etc.set_crosshair_visibility, is_visible=True)),   
    "hide crosshair": R(Function(etc.set_crosshair_visibility, is_visible=False)),
    "enable [cursor] snapping": R(Function(etc.set_cursor_snapping, is_enabled=True)),   
    "disable [cursor] snapping": R(Function(etc.set_cursor_snapping, is_enabled=False)),   
    "head [tracking] [<head_tracking_type>]": R(Function(etc.set_head_tracking)),
    "disable head [tracking]": R(Function(etc.disable_head_tracking)),    
    "set display <monitor_number>": R(Function(etc.set_display)),
    "[cursor] speed <cursor_speed>": R(Function(etc.set_cursor_speed)),   
    "(launch|run) eye tracker": R(Function(etc.launch_eye_tracking_app)),    
    "(close|exit) eye tracker": R(Function(etc.close_eye_tracker)),
    } 
    extras = [
        Dictation("text"),
        IntegerRefST("stroke_size", 1, 201),        
        IntegerRefST("font_size", 1, 30),        
        IntegerRefST("column_number", 1, 64),        
        IntegerRefST("row_number", 1, 40),        
        IntegerRefST("opacity", 0, 101),        

        IntegerRefST("cursor_speed", 1, 21),
        IntegerRefST("monitor_index", 0, 3),
        IntegerRefST("monitor_number", 1, 4),
        IntegerRefST("n", 0, 1001),     
        #IntegerRefST("nn", 1, 10),
        IntegerRefST("nnn", 1, 5),
        
        Choice("direction", {
            "up left|sauce lease": 1,
            "up|sauce": 2,
            "up right|sauce ross": 3,
            "left|lease": 4,
            "centre": 5,
            "right|ross": 6,
            "down left|dunce ross": 7,
            "down|dunce": 8,
            "down right|dunce ross": 9
        }),
        Choice("time_in_milliseconds", {
            "super slow": 1000,
            "slow": 500,
            "normal": 100,
            "fast": 50,
            "very fast": 25,
            "hyperspeed": 15
        }),
        Choice("scrolling_interactor_position", {
            "left": 0,
            "middle|mid": 1,
            "right": 2
        }), 
        Choice("is_horizontal", {
            "vertical": False,
            "horizontal": True
        }),
        Choice("word_snap_option", {
            "go [to]": 0,
            "click": 1,
            "grab|select": 2,
            "go [in] front|go before": 3,
            "go [to] back|go (behind|after)": 4,
        }),
        Choice("head_tracking_type", {
            "both": 0,
            "vertical": 1,
            "horizontal": 2,    
        }),
    ]

    defaults = {            
            "monitor_index": -1,
            "is_horizontal": False,
            "word_snap_option": 0,
            "head_tracking_type": 0,
            "text": "",
        }
        
def get_rule():
    return EyeTracking, RuleDetails(name="Eye Tracking")
                                       



