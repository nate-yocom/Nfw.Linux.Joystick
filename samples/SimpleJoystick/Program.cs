using Nfw.Linux.Joystick.Simple;

using(Joystick joystick = new Joystick("/dev/input/js0")) {
    joystick.ButtonCallback = (j, button, pressed) => {
        Console.WriteLine($"{j.DeviceName} => Button[{button}] => {pressed}");
    };

    joystick.AxisCallback = (j, axis, value) => {
        Console.WriteLine($"{j.DeviceName} => Axis[{axis}] => {value}");
    };

    joystick.ConnectedCallback = (j, c) => {
        Console.WriteLine($"{j.DeviceName} => Connected[{c}]");
    };

    Console.WriteLine("Watching for js0 events, press enter to quit...");
    Console.ReadLine();
}

