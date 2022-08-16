namespace Nfw.Linux.Joystick.Smart {
    [Flags]
    public enum ButtonEventTypes 
    {
            None        = 0x00,
            Press       = 0x01,
            Release     = 0x02,            
            ShortPress  = 0x04,   // Press -> release
            LongPress   = 0x08,   // The result of a Press -> Release with a defined 'hold' threshold
            All         = 0xFF
    }
}