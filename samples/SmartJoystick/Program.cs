using Nfw.Linux.Joystick.Smart;

string jsFile = args.Count() > 0 ? args[0] : "/dev/input/js0";

using(Joystick joystick = new Joystick(jsFile, ButtonEventTypes.All)) {

    // We can optionally set the behavior controller wide, or per button, re what constitutes a LongPress vs a ShortPress:
    //  To do this per-button, call joystick.SetButtonSettings(<button id>, <settings>);    
    joystick.DefaultButtonSettings = new ButtonSettings() { 
        LongPressMinimumDurationMilliseconds = 500
    };

    joystick.ButtonCallback = (j, button, eventType, pressed, elapsedTime) => {
        Console.WriteLine($"{j.DeviceName} => Button[{button}] => {eventType} [Current: {pressed} Elapsed: {elapsedTime}]");
    };

    joystick.AxisCallback = (j, axis, value, elapsedTime) => {
        Console.WriteLine($"{j.DeviceName} => Axis[{axis}] => {value} [Elapsed: {elapsedTime}]");
    };

    joystick.ConnectedCallback = (j, c) => {
        Console.WriteLine($"{j.DeviceName} => Connected[{c}]");
    };

    Console.WriteLine($"Watching for {jsFile} events, press enter to quit...");
    Console.ReadLine();
}