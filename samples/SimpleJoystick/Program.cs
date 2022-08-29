using Nfw.Linux.Joystick.Simple;

string jsFile = args.Count() > 0 ? args[0] : "/dev/input/js0";

using(Joystick joystick = new Joystick(jsFile)) {
    joystick.ButtonCallback = (j, button, pressed) => {
        Console.WriteLine($"{j.DeviceName} => Button[{button}] => {pressed}");
    };

    joystick.AxisCallback = (j, axis, value) => {
        Console.WriteLine($"{j.DeviceName} => Axis[{axis}] => {value}");
    };

    joystick.ConnectedCallback = (j, c) => {
        Console.WriteLine($"{j.DeviceName} => Connected[{c}]");
    };
    
    Console.WriteLine($"Watching for {jsFile} events, press enter to quit...");
    Console.ReadLine();
}