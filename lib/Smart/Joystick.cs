using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Nfw.Linux.Joystick.Simple;

namespace Nfw.Linux.Joystick.Smart {
    
    public class Joystick : Nfw.Linux.Joystick.Simple.Joystick {
        // this, ID, EventType, Value, Duration Since Last
        public new Action<Joystick, byte, ButtonEventTypes, bool, TimeSpan>? ButtonCallback;
        // this, ID, Value, Duration Since Last
        public new Action<Joystick, byte, short, TimeSpan>? AxisCallback;
        // Settings used for timing in the absence of a button-specific setting
        public ButtonSettings DefaultButtonSettings { get; set; } = new ButtonSettings();                           
        public ButtonEventTypes SubscribedEvents { get; set; } = ButtonEventTypes.All;


        private bool _disposedValue = false;
        private Stopwatch _stopwatch = new Stopwatch();
        private Dictionary<byte, TimeSpan> _buttonState = new Dictionary<byte, TimeSpan>();
        private Dictionary<byte, TimeSpan> _axisState = new Dictionary<byte, TimeSpan>();
        private Dictionary<byte, ButtonSettings> _buttonSettings = new Dictionary<byte, ButtonSettings>();        


        public Joystick(string deviceFile, ILogger? logger, ButtonEventTypes subscribeTo) : base(deviceFile, logger) {
            SubscribedEvents = subscribeTo;
            _stopwatch.Start();
        }

        public Joystick(string deviceFile, ButtonEventTypes subscribeTo) : this(deviceFile, null, subscribeTo) {            
        }

        public Joystick(ILogger logger, ButtonEventTypes subscribeTo) : this(DEFAULT_DEVICE, logger, subscribeTo) {
        }

        public Joystick(ButtonEventTypes subscribeTo) : this(DEFAULT_DEVICE, null, subscribeTo) {            
        }
        
        public Joystick() : this(DEFAULT_DEVICE, null, ButtonEventTypes.All) {
        }        

        public void SetButtonSettings(byte button, ButtonSettings settings) {
            lock (_buttonSettings) {
                _buttonSettings[button] = settings;
            }
        }

        public void ClearButtonSettings() {
            lock (_buttonSettings) {
                _buttonSettings.Clear();
            }
        }

        public void ClearButtonSettings(byte button) {
            lock (_buttonSettings) {
                _buttonSettings.Remove(button);
            }
        }

        private ButtonSettings GetButtonSettings(byte button) {
            lock(_buttonSettings) {
                return _buttonSettings.ContainsKey(button) ? _buttonSettings[button] : DefaultButtonSettings;
            }
        }
        
        private bool IsSubscribedTo(ButtonEventTypes buttonEvent) {
            return (SubscribedEvents & buttonEvent) != ButtonEventTypes.None;
        }
  
        protected virtual void ButtonChangeCallback(byte button, bool pressed) {
            // If the button isnt known yet, we add it - and ignore the event (always ignore init basically)
            if (!_buttonState.ContainsKey(button)) {                
                _buttonState[button] = _stopwatch.Elapsed;
                return;
            }            

            TimeSpan lastButtonEventElapsedTime = _buttonState[button];
            ButtonSettings settings = GetButtonSettings(button);
            
            // Every event we (re)start the clock, as we only ever care about delta between events            
            TimeSpan timeSinceLastEvent = _stopwatch.Elapsed - lastButtonEventElapsedTime;
            _buttonState[button] = _stopwatch.Elapsed;            

            // Ignore dupes
            if (ButtonValue(button) == pressed) {                
                return;
            }
            
            // Short click is any transition from press->release that is at least short click duration
            //  but not more than long click duration
            if (IsSubscribedTo(ButtonEventTypes.ShortPress) && pressed == false &&
                    timeSinceLastEvent.TotalMilliseconds >= settings.GetShortClickMinimumDurationMilliseconds() &&
                    timeSinceLastEvent.TotalMilliseconds < settings.GetLongClickMinimumDurationMilliseconds()) {
                        InvokeSmartButtonCallback(button, ButtonEventTypes.ShortPress, pressed, timeSinceLastEvent);
            }                            
            
            // If we are releasing, and it has been long enough, then generate a long click
            if (IsSubscribedTo(ButtonEventTypes.LongPress) && pressed == false && 
                timeSinceLastEvent.TotalMilliseconds >= settings.GetLongClickMinimumDurationMilliseconds()) {
                InvokeSmartButtonCallback(button, ButtonEventTypes.LongPress, pressed, timeSinceLastEvent);
            } 
            
            if (IsSubscribedTo(ButtonEventTypes.Press) && pressed) {
                InvokeSmartButtonCallback(button, ButtonEventTypes.Press, pressed, timeSinceLastEvent);
            }

            if (IsSubscribedTo(ButtonEventTypes.Release) && !pressed) {
                InvokeSmartButtonCallback(button, ButtonEventTypes.Release, pressed, timeSinceLastEvent);
            }
        }
        
        protected virtual void AxisChangeCallback(byte axis, short value) {            
            // If the axis isnt known yet, we add it - and ignore the event (always ignore init basically)
            if (!_axisState.ContainsKey(axis)) {
                _axisState[axis] = _stopwatch.Elapsed;
                return;
            }            

            TimeSpan lastAxisEventElapsedTime = _axisState[axis];
            
            // Every event we (re)start the clock, as we only ever care about delta between events            
            TimeSpan timeSinceLastEvent = _stopwatch.Elapsed - lastAxisEventElapsedTime;
            _axisState[axis] = _stopwatch.Elapsed;

            // Ignore dupes
            if (AxisValue(axis) == value) {
                return;
            }
            
            InvokeSmartAxisCallback(axis, value, timeSinceLastEvent);
        }

        protected virtual void InvokeSmartButtonCallback(byte key, ButtonEventTypes eventType, bool pressed, TimeSpan elapsed) {
            ButtonCallback?.Invoke(this, key, eventType, pressed, elapsed);
        }

        protected virtual void InvokeSmartAxisCallback(byte axis, short value, TimeSpan elapsed) {
            AxisCallback?.Invoke(this, axis, value, elapsed);
        }

        protected override void InvokeButtonCallback(byte key, bool value) {            
            ButtonChangeCallback(key, value);
        }

        protected override void InvokeAxisCallback(byte key, short value) {            
            AxisChangeCallback(key, value);
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