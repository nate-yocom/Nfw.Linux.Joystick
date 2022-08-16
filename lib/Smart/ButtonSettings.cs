namespace Nfw.Linux.Joystick.Smart {
    public class ButtonSettings {
        // How long must button have been held to cause a long click on release    
        public int? LongPressMinimumDurationMilliseconds { get; set; } = null;
        public int? ShortPressMinimumDurationMilliseconds { get; set; } = null;
        public int? ShortPressMaximumDurationMillseconds { get; set; } = null;
        
        public void MergeSettings(ButtonSettings? rhs) {
            if (rhs == null) return;            
            if(rhs.LongPressMinimumDurationMilliseconds != null) LongPressMinimumDurationMilliseconds = rhs.LongPressMinimumDurationMilliseconds;
            if(rhs.ShortPressMinimumDurationMilliseconds != null) ShortPressMinimumDurationMilliseconds = rhs.ShortPressMinimumDurationMilliseconds;
            if(rhs.ShortPressMaximumDurationMillseconds != null) ShortPressMaximumDurationMillseconds = rhs.ShortPressMaximumDurationMillseconds;            
        }
        
        public int GetLongClickMinimumDurationMilliseconds() {
            return LongPressMinimumDurationMilliseconds ?? DefaultSettings?.LongPressMinimumDurationMilliseconds ?? DEFAULT_LONG_CLICK_MINIMUM;
        }

        public int GetShortClickMinimumDurationMilliseconds() {
            return ShortPressMinimumDurationMilliseconds ?? DefaultSettings?.ShortPressMinimumDurationMilliseconds ?? DEFAULT_SHORT_CLICK_MINIMUM;
        }        
        private const int DEFAULT_LONG_CLICK_MINIMUM = 1000;
        private const int DEFAULT_SHORT_CLICK_MINIMUM = 0;
        
        public static ButtonSettings DefaultSettings { get; set; } = new ButtonSettings();        
    }    
}