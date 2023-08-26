# Nfw.Linux.Joystick

A Joystick/Gamepad Library with generic support for the Linux input system and the /dev/input/js<x> device(s).  Original inspiration from https://github.com/nahueltaibo/gamepad - many thanks and attribution due to that repo and its original parsing code.

This library extends and goes beyond however, providing 3 layers with which you can interact with controllers:

- `Simple`: You tell the library what /dev/input/js<x> to read, and provide a callback for Button and Axis events
- `Smart`: Builds on Simple and let's you subscribe to specific events, including synthetic things like "ShortPress" vs "LongPress" (see full list here: https://github.com/nate-yocom/Nfw.Linux.Joystick/blob/main/lib/Smart/ButtonEventTypes.cs)
- `Xpad`: Builds on Smart layer, with direct mapping for supporting XBox controllers (as supported by the Linux xpad driver), as well as support for mapping Axis events to buttons (i.e. a Button event for DPadLeft vs DPadRight)

## All Layers

 - Providing a ```Microsoft.Extensions.Logging.ILogger``` (perhaps via IoC) at construction time
 - Retrieving the name of the attached device (as provided by the manufacturer/device in response to an ioctl)
 - Callback on connect/disconnect (and no events when disconnected)
 - Current connected state available via the ```Connected``` property
 - Current device being watched via the ```Device``` property

## NuGet

```dotnet add package Nfw.Linux.Joystick```
 
## Samples 

See samples for each layer: https://github.com/nate-yocom/Nfw.Linux.Joystick/tree/main/samples
 
## Simple Layer

This layer is, as the name suggests, the simplest, with no real configuration beyond providing the path to the device file to read from (and watch for connect/disconnect):

```csharp
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
```

## Smart Layer

This layer adds the ability to report events with more meaning - i.e. Short vs Long press (avoiding the need for the library user to do timing and state tracking).  This also introduces a per-axis or button elapsed time since last event argument to the callback, allowing for custom timing logic in user code.

```csharp
using Nfw.Linux.Joystick.Smart;

using(Joystick joystick = new Joystick("/dev/input/js0", ButtonEventTypes.All)) {

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

    Console.WriteLine("Watching for js0 events, press enter to quit...");
    Console.ReadLine();
}
```

Note that the second argument to this constructor allows for masking in/out specific events as desired.

## Xpad Layer

This layer is idea for the case where you know the attached controller is an XBox controller - as supported by the Xpad driver for Linux.  Optionally, calling code can also ask that all Axis be mapped as button events.

```csharp
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
```

Note that this layer also includes support for the Profile button specific to the Microsoft X-Box Adaptive Controller, which requires a newer Xpad driver than currently in any mainline Linux tree.  See https://github.com/nate-yocom/xpad-xac for such a driver.

## References
- Gamepad library: https://github.com/nahueltaibo/gamepad by @nahueltaibo
- Linux kernel joystick API: https://www.kernel.org/doc/Documentation/input/joystick-api.txt

## Changes

- 1.0.2 adds autoprobe on construction for device name
- 1.0.1 fixes device access to be non-exclusive and read only
