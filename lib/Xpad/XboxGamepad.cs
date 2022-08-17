using Microsoft.Extensions.Logging;

using Nfw.Linux.Joystick.Smart;

namespace Nfw.Linux.Joystick.Xpad {
    public class XboxGamepad : Nfw.Linux.Joystick.Smart.Joystick {
        // this, ID, EventType, Value, Duration Since Last
        public new Action<XboxGamepad, Button, ButtonEventTypes, bool, TimeSpan>? ButtonCallback;
        // this, ID, Value, Duration Since Last
        public new Action<XboxGamepad, Axis, short, TimeSpan>? AxisCallback;

        public bool TreatAxisAsButtons { get; set; } = false;

        private bool _disposedValue = false;
        
        private const short AXIS_NEGATIVE_MAX = -32767;
        private const short AXIS_POSITIVE_MAX = 32767;
        
        public float MappedAxisButtonPressThreshold { get; set; }= 0.90f;  // What % of MAX must be hit by an axis to be considered a button press?
        public short MappedAxisButtonCenterForgiveness {get; set; } = 1024; // Within what distance of 0 is an axis treated as 0 (centered)

        public XboxGamepad(string deviceFile, ILogger? logger, ButtonEventTypes subscribeTo) : base(deviceFile, logger, subscribeTo) {
            // We init all possible mapped axis->buttons just in case
            foreach(Button button in Enum.GetValues<Button>()) {
                if (button > Button.StartSyntheticButtons) {
                    InitButton((byte) button, false);
                }
            }
        }

        public XboxGamepad(string deviceFile, ButtonEventTypes subscribeTo) : this(deviceFile, null, subscribeTo) {            
        }

        public XboxGamepad(ILogger logger, ButtonEventTypes subscribeTo) : this(DEFAULT_DEVICE, logger, subscribeTo) {
        }

        public XboxGamepad(ButtonEventTypes subscribeTo) : this(DEFAULT_DEVICE, null, subscribeTo) {            
        }
        
        public XboxGamepad() : this(DEFAULT_DEVICE, null, ButtonEventTypes.All) {
        }        

        public void SetButtonSettings(Button button, ButtonSettings settings) {
            base.SetButtonSettings((byte) button, settings);
        }
        
        public void ClearButtonSettings(Button button) {
            base.ClearButtonSettings((byte) button);
        }

        protected override void ButtonChangeCallback(byte button, bool pressed) {
            // The simple joystick wont track our mapped buttons current state,
            //  so we have to insert the value ourselves, AFTER the callback happens
            base.ButtonChangeCallback(button, pressed);
            if (button > (byte) Button.StartSyntheticButtons) {
                SetButtonValue(button, pressed);
            }
        }

        protected override void AxisChangeCallback(byte axis, short value) {
            // If we are treating axis normally, just call base class
            if (!TreatAxisAsButtons) {
                base.AxisChangeCallback(axis, value);
                return;
            }

            // Map to button, then fire that instead
            Button mappedButton = ButtonFromAxisAndDirection((Axis) axis, value);
            if (mappedButton == Button.StartSyntheticButtons) {                
                _logger?.LogWarning($"Uknown Axis->Button mapping for Axis: {axis} and Value: {value}");
                return;
            }
            
            // Now handle according to each button            
            switch(mappedButton) {
                case Button.LStickLeft:
                case Button.LStickRight:
                    StandardAxisMap(Button.LStickLeft, Button.LStickRight, value);
                    break;
                case Button.LStickUp:
                case Button.LStickDown:
                    StandardAxisMap(Button.LStickUp, Button.LStickDown, value);
                    break;
                case Button.RStickLeft:
                case Button.RStickRight:
                    StandardAxisMap(Button.RStickLeft, Button.RStickRight, value);
                    break;
                case Button.RStickUp:
                case Button.RStickDown:
                    StandardAxisMap(Button.RStickUp, Button.RStickDown, value);
                    break;
                case Button.DPadLeft:
                case Button.DPadRight:
                    StandardAxisMap(Button.DPadLeft, Button.DPadRight, value);
                    break;
                case Button.DPadUp:
                case Button.DPadDown:
                    StandardAxisMap(Button.DPadUp, Button.DPadDown, value);
                    break;
                case Button.LT:
                case Button.RT:
                    // Full press only counts, to avoid multiple events
                    if (value <= (MappedAxisButtonPressThreshold * AXIS_NEGATIVE_MAX))
                        ButtonChangeCallback((byte) mappedButton, false);
                    else if (value >= (MappedAxisButtonPressThreshold * AXIS_POSITIVE_MAX))
                        ButtonChangeCallback((byte) mappedButton, true);
                    break;
                case Button.Profile0:
                    ButtonChangeCallback((byte) Button.Profile0, true);
                    ButtonChangeCallback((byte) Button.Profile1, false);
                    ButtonChangeCallback((byte) Button.Profile2, false);
                    ButtonChangeCallback((byte) Button.Profile3, false);
                    break;
                case Button.Profile1:
                    ButtonChangeCallback((byte) Button.Profile0, false);
                    ButtonChangeCallback((byte) Button.Profile1, true);
                    ButtonChangeCallback((byte) Button.Profile2, false);
                    ButtonChangeCallback((byte) Button.Profile3, false);
                    break;
                case Button.Profile2:
                    ButtonChangeCallback((byte) Button.Profile0, false);
                    ButtonChangeCallback((byte) Button.Profile1, false);
                    ButtonChangeCallback((byte) Button.Profile2, true);
                    ButtonChangeCallback((byte) Button.Profile3, false);
                    break;
                case Button.Profile3:
                    ButtonChangeCallback((byte) Button.Profile0, false);
                    ButtonChangeCallback((byte) Button.Profile1, false);
                    ButtonChangeCallback((byte) Button.Profile2, false);
                    ButtonChangeCallback((byte) Button.Profile3, true);
                    break;
            }
        }

        private void StandardAxisMap(Button negative, Button positive, short value) {
            if (Math.Abs(value) <= MappedAxisButtonCenterForgiveness) {
                // This is a release of both dirs, have to signal both released - dupes removed at handler layer                
                ButtonChangeCallback((byte) negative, false);
                ButtonChangeCallback((byte) positive, false);                        
            } else if (value <= (MappedAxisButtonPressThreshold * AXIS_NEGATIVE_MAX)) {
                ButtonChangeCallback((byte) negative, true);
            } else if (value >= (MappedAxisButtonPressThreshold * AXIS_POSITIVE_MAX)) {
                ButtonChangeCallback((byte) positive, true);
            }
        }

        protected override void InvokeSmartButtonCallback(byte key, ButtonEventTypes eventType, bool pressed, TimeSpan elapsed) {
            ButtonCallback?.Invoke(this, (Button) key, eventType, pressed, elapsed);            
        }

        protected override void InvokeSmartAxisCallback(byte axis, short value, TimeSpan elapsed) {
            AxisCallback?.Invoke(this, (Axis) axis, value, elapsed);            
        }

        private Button ButtonFromAxisAndDirection(Axis axis, short direction) {
            switch(axis) {
                case Axis.LStickLeftRight:
                    return direction < 0 ? Button.LStickLeft : Button.LStickRight;
                case Axis.LStickUpDown:
                    return direction < 0 ? Button.LStickUp : Button.LStickDown;
                case Axis.LT:
                    return Button.LT;
                case Axis.RStickLeftRight:
                    return direction < 0 ? Button.RStickLeft : Button.RStickRight;
                case Axis.RStickUpDown:
                    return direction < 0 ? Button.RStickUp : Button.RStickDown;
                case Axis.RT:
                    return Button.RT;
                case Axis.DPadLeftRight:
                    return direction < 0 ? Button.DPadLeft : Button.DPadRight;
                case Axis.DPadUpDown:
                    return direction < 0 ? Button.DPadUp : Button.DPadDown;
                case Axis.Profile:
                    switch(direction) {
                        case -32767:
                            return Button.Profile0;
                        case -16384:
                            return Button.Profile1;
                        case 0:
                            return Button.Profile2;
                        case 16384:
                            return Button.Profile3;                            
                        default:
                            // Unknown
                            return Button.StartSyntheticButtons;
                    }
            }

            // Unknown?
            return Button.StartSyntheticButtons;
        } 

        protected override void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    ButtonCallback = null;
                    AxisCallback = null;                    
                }
                
                _disposedValue = true;
            }
        }
    }
}