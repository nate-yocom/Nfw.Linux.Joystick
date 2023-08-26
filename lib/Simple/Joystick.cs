using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

namespace Nfw.Linux.Joystick.Simple {

    public class Joystick : IDisposable {
        public Action<Joystick, byte, bool>? ButtonCallback;
        public Action<Joystick, byte, short>? AxisCallback;
        public Action<Joystick, bool>? ConnectedCallback;
        
        private bool _disposedValue = false;        
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();        
        private readonly string _deviceFile;
        private Dictionary<byte, bool> _buttons = new Dictionary<byte, bool>();
        private Dictionary<byte, short> _axis = new Dictionary<byte, short>();
        
        protected ILogger? _logger;
        protected const string DEFAULT_DEVICE = "/dev/input/js0";        

        public TimeSpan RetryDeviceInterval { get; set; } = TimeSpan.FromSeconds(1);
        public bool Connected { get; private set; } = false;
        public string? DeviceName { get; private set; }
        public bool CallbackForAllEvents { get; set; } = true;
        public string Device { get { return _deviceFile; } }
        
        public Joystick(string deviceFile, ILogger? logger) {
            _deviceFile = deviceFile;
            _logger = logger;

            DeviceName = ProbeForName();
            
            Task.Factory.StartNew(() => RunningLoop(_cancellationTokenSource.Token));
        }

        public Joystick(string deviceFile) : this(deviceFile, null) {            
        }

        public Joystick(ILogger logger) : this(DEFAULT_DEVICE, logger) {            
        }

        public Joystick() : this(DEFAULT_DEVICE, null) {            
        }

        public bool ButtonValue(byte button) {
            return _buttons[button];
        }

        public short AxisValue(byte axis) {
            return _axis[axis];
        }

        protected void InitButton(byte button, bool value) {
            _buttons[button] = value;
        }

        protected void SetButtonValue(byte button, bool value) {
            _buttons[button] = value;
        }

        protected void InitAxis(byte axis, short value) {
            _axis[axis] = value;
        }

        protected bool KnownButton(byte button) {
            return _buttons.ContainsKey(button);
        }

        private void RunningLoop(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                try {
                    if (Connected) {
                        // Try to read messages until it fails...
                        ProcessDeviceMessages(token);
                    } else {
                        // Try to re-open/probe device if exists
                        if (DeviceFileExists) {
                            DeviceName = ProbeForName();
                            Connected = true;
                            InvokeConnectedCallback(Connected);                            
                        } else {
                            Thread.Sleep(RetryDeviceInterval);
                        }
                    }
                } catch (Exception ex) {                    
                    _logger?.LogError($"Unexpected error in reading from {_deviceFile}: {ex.Message}");
                    Connected = false;
                    InvokeConnectedCallback(Connected);
                }
            }
        }

        private void ProcessDeviceMessages(CancellationToken token) {            
            using (FileStream fs = new FileStream(_deviceFile, FileMode.Open, FileAccess.Read)) {                
                byte[] message = new byte[MessageParser.ReadSize];
                while (!token.IsCancellationRequested) {                    
                    fs.Read(message, 0, MessageParser.ReadSize);

                    if (message.IsConfiguration()) {
                        ProcessConfiguration(message);
                    }

                    ProcessValues(message);
                }
            }
        }

        private void ProcessConfiguration(byte[] message) {
            byte key = message.Id();
            if (message.IsButton()) {                
                InitButton(key, false);
            } else if (message.IsAxis()) {
                InitAxis(key, 0);
            }
        }

        private void ProcessValues(byte[] message) {
            byte key = message.Id();

            if (message.IsButton()) {
                bool oldValue = _buttons[key];
                bool newValue = message.ButtonValue();
                
                if (CallbackForAllEvents || oldValue != newValue) {
                    // Note, important to update the _buttons value *AFTER* the callback, so callback can compare against current state if useful
                    InvokeButtonCallback(key, newValue);
                    _buttons[key] = newValue;
                }
            } else if (message.IsAxis()) {
                short oldValue = _axis[key];
                short newValue = message.AxisValue();

                if (CallbackForAllEvents || oldValue != newValue) {
                    // Note, important to update the _buttons value *AFTER* the callback, so callback can compare against current state if useful
                    InvokeAxisCallback(key, newValue);
                    _axis[key] = newValue;
                }
            }            
        }

        protected virtual void InvokeButtonCallback(byte key, bool value) {
            if (!_cancellationTokenSource.Token.IsCancellationRequested) 
                ButtonCallback?.Invoke(this, key, value);
        }

        protected virtual void InvokeAxisCallback(byte key, short value) {
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
                AxisCallback?.Invoke(this, key, value);
        }

        protected virtual void InvokeConnectedCallback(bool value) {
            if (!_cancellationTokenSource.Token.IsCancellationRequested) 
                ConnectedCallback?.Invoke(this, value);
        }

        private bool DeviceFileExists { get { return File.Exists(_deviceFile); } }

        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        private static extern int ioctl(int handle, uint request, byte[] output);

        [DllImport("libc", EntryPoint = "__errno_location")]
        private static extern System.IntPtr __errno_location();
        private const uint JSIOCGNAME_128 = 0x80806A13;  // JSIOCGNAME(len = 128)

        private string? ProbeForName() {
            try {                
                using(var fileHandle = File.OpenHandle(_deviceFile, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, 0)) {
                    byte[] name = new byte[128];
                    if(ioctl(fileHandle.DangerousGetHandle().ToInt32(), JSIOCGNAME_128, name) < 0) {
                        _logger?.LogError($"ProbeForName ioctl({JSIOCGNAME_128}) error: {System.Runtime.InteropServices.Marshal.ReadInt32(__errno_location())}");
                        return null;
                    } else {
                        string stringName = System.Text.ASCIIEncoding.ASCII.GetString(name).TrimEnd(new char[] { '\r', '\n', ' ', '\0' });
                        _logger?.LogInformation($"Found Joystick at {_deviceFile} => {stringName}");
                        return stringName;
                    }                    
                }
            } catch(Exception ex) {
                _logger?.LogError($"Unable to probe {_deviceFile}: {ex.Message}");
                return null;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    ButtonCallback = null;
                    AxisCallback = null;
                    ConnectedCallback = null;
                    _cancellationTokenSource.Cancel();
                }
                
                _disposedValue = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

