using Nfw.Linux.Joystick.Smart;
using Nfw.Linux.Joystick.Xpad;

using(XboxGamepad joystick = new XboxGamepad("/dev/input/js0", ButtonEventTypes.All)) {
    // We can optionally set the behavior controller wide, or per button, re what constitutes a LongPress vs a ShortPress:
    //  To do this per-button, call joystick.SetButtonSettings(<button id>, <settings>);    
    joystick.DefaultButtonSettings = new ButtonSettings() { 
        LongPressMinimumDurationMilliseconds = 500
    };

    // We can optionally turn all axis events into buttons if we want:
    //      joystick.TreatAxisAsButtons = true;
    //
    // When mapping to buttons, we can:
    //
    //      Adjust the deadzone (center) for axis as a distance from 0 with:
    //          joystick.MappedAxisButtonCenterForgiveness = 1024;
    //
    //      Adjust the % of max which constitutes a button press when mapping an axis to a button    
    //          joystick.MappedAxisButtonPressThreshold = 0.90f;

    joystick.ButtonCallback = (j, button, eventType, pressed, elapsedTime) => {
        Console.WriteLine($"{j.DeviceName} => Button[{button}] => {eventType} [Current: {pressed} Elapsed: {elapsedTime}]");
    };

    joystick.AxisCallback = (j, axis, value, elapsedTime) => {
        Console.WriteLine($"{j.DeviceName} => Axis[{axis}] => {value} [Elapsed: {elapsedTime}]");
    };

    joystick.ConnectedCallback = (j, c) => {
        Console.WriteLine($"{j.DeviceName} => Connected[{c}]");
    };

    Console.WriteLine("Watching for js0 events, press enter to quit...");
    Console.ReadLine();
}