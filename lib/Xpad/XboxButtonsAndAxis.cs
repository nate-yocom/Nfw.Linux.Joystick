namespace Nfw.Linux.Joystick.Xpad {
    public enum Button {
        A       = 0x00,
        B       = 0x01,
        X       = 0x02,
        Y       = 0x03,
        LB      = 0x04,
        RB      = 0x05,
        Window  = 0x06,
        Menu    = 0x07,
        Xbox    = 0x08,
        L       = 0x09,
        R       = 0x0A,

        // Only if TreatAxisAsButtons is used (these numbers are simulated)
        StartSyntheticButtons = 0xA0,        
        LStickLeft  = StartSyntheticButtons + 1,    // Axis 0x00 - -32767 = Left, 0 = unpressed
        LStickRight = StartSyntheticButtons + 2,    // Axis 0x00 - 32767 = Right, 0 = unpressed
        LStickUp    = StartSyntheticButtons + 3,    // Axis 0x01 - -32767 = Up, 0 = unpressed
        LStickDown  = StartSyntheticButtons + 4,    // Axis 0x01 - 32767 = Down, 0 = unpressed
        LT          = StartSyntheticButtons + 5,    // Axis 0x02 - 32767 = pressed, -32767 = unpressed
        RStickLeft  = StartSyntheticButtons + 6,    // Axis 0x03 - -32767 = Left, 0 = unpressed
        RStickRight = StartSyntheticButtons + 7,    // Axis 0x03 - 32767 = Right, 0 = unpressed        
        RStickUp    = StartSyntheticButtons + 8,    // Axis 0x04 - -32767 = Left, 0 = unpressed
        RStickDown  = StartSyntheticButtons + 9,    // Axis 0x04 - 32767 = Right, 0 = unpressed        
        RT          = StartSyntheticButtons + 10,   // Axis 0x05 - 32767 = pressed, -32767 = unpressed
        DPadLeft    = StartSyntheticButtons + 11,   // Axis 0x06 - -32767 = pressed, 0 = unpressed
        DPadRight   = StartSyntheticButtons + 12,   // Axis 0x06 - 32767 = pressed, 0 = unpressed
        DPadUp      = StartSyntheticButtons + 13,   // Axis 0x07 - -32767 = pressed, 0 = unpressed
        DPadDown    = StartSyntheticButtons + 14,   // Axis 0x07 - 32767 = pressed, 0 = unpressed
        Profile0    = StartSyntheticButtons + 15,   // Axis 0x08 - -32767
        Profile1    = StartSyntheticButtons + 16,   // Axis 0x08 - -16384
        Profile2    = StartSyntheticButtons + 17,   // Axis 0x08 - 0
        Profile3    = StartSyntheticButtons + 18,   // Axis 0x08 - 16384        
    }

    public enum Axis {
        LStickLeftRight = 0x00, // -32767 = Left, 0 = Center, 32767 = Right
        LStickUpDown    = 0x01, // -32767 = Up, 0 = Center, 32767 = Down.  XAC also exposes X1 (Up) and X2 (Down) as 0x01
        LT              = 0x02, // Full range, -32767 unpressed to 32767 fully pressed
        RStickLeftRight = 0x03, // -32767 = Left, 0 = Center, 32767 = Right
        RStickUpDown    = 0x04, // -32767 = Up, 0 = Center, 32767 = Down
        RT              = 0x05, // Full range, -32767 unpressed to 32767 fully pressed
        DPadLeftRight   = 0x06, // 0x06 (negative = Left, positive = Right)
        DPadUpDown      = 0x07, // 0x07 (negative = Down, positive = Up)        
        Profile         = 0x08, // Only on the XAC: 4 values, -32767 = 0, -16384 = 1, 0 = 2, 16384 = 3 
    }    
}