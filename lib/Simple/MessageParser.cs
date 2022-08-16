namespace Nfw.Linux.Joystick.Simple {
    internal static class MessageParser {
        
        [Flags]
        private enum MessageFlags {
            Configuration   = 0x80,
            Button          = 0x01,
            Axis            = 0x02
        }

        private const int MESSAGE_SIZE = 8;
        private const int MESSAGE_FLAG_INDEX = 6;
        private const int PRESSED_FLAG_INDEX = 4;
        private const int ADDRESS_INDEX = 7;
        private const int AXIS_INDEX_START = 4;

        public static int ReadSize { get { return MESSAGE_SIZE; } }

        public static bool IsConfiguration(this byte[] message) { 
            return HasFlag(message[MESSAGE_FLAG_INDEX], MessageFlags.Configuration);
        }

        public static bool IsButton(this byte[] message) {
            return HasFlag(message[MESSAGE_FLAG_INDEX], MessageFlags.Button);
        }

        public static bool IsAxis(this byte[] message) {
            return HasFlag(message[MESSAGE_FLAG_INDEX], MessageFlags.Axis);
        }        

        public static byte Id(this byte[] message) {
            return message[ADDRESS_INDEX];
        }

        public static short AxisValue(this byte[] message) {
            return BitConverter.ToInt16(message, AXIS_INDEX_START);
        }

        public static bool ButtonValue(this byte[] message) {
            return message[PRESSED_FLAG_INDEX] == 0x01;
        }

        private static bool HasFlag(byte value, MessageFlags flag) {
            return (value & (byte)flag) == (byte)flag;
        }
    }
}