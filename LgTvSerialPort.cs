using System;
using System.IO;
using System.Threading;
using System.IO.Ports;
using System.Globalization;
using System.Text;
using LagrangianDesign.LgTvControl.AssortedExtensions;
using static System.IO.Path;
using static System.StringComparison;
using static System.Globalization.NumberStyles;

namespace LagrangianDesign.LgTvControl {
    public sealed class LgTvSerialPort : IDisposable {
        public LgTvSerialPort(String name) {
            if (name is null) {
                throw new ArgumentNullException(nameof(name));
            }
            _Port = new SerialPort(name);
        }

        SerialPort _Port;
        SerialPort Port { 
            get {
                if (_Port == null) {
                    throw new ObjectDisposedException(nameof(LgTvSerialPort));
                }
                if (!_Port.IsOpen) {
                    _Port.BaudRate = 9600;
                    _Port.DataBits = 8;
                    _Port.Parity = Parity.None;
                    _Port.StopBits = StopBits.One;
                    _Port.ReadTimeout = 2000;
                    _Port.WriteTimeout = 2000;
                    _Port.Open();
                }
                return _Port;
            }
        }

        Byte SendCommand(String commandCode, Byte data) {
            if (commandCode?.Length != 2) {
                throw new ArgumentException($"Invalid command code {commandCode.Quoted()}.", nameof(commandCode));
            }
            var port = Port;
            lock (port) {
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                var command = $"{commandCode} 00 {data:X2}\x0D";
                port.Write(command);
                var response = port.ReadTo("x");
                if (response.Length == 9
                    && response.StartsWith(command[1])
                    && response[5..^2].Equals("OK", Ordinal)
                    && Byte.TryParse(response[7..^0], HexNumber, CultureInfo.InvariantCulture, out var result)) {
                    return result;
                }
                throw new IOException($"Response {response.Quoted()} to command {command[0..^1].Quoted()} not OK.");
            }
        }

        Byte GetPercent(String commandCode) {
            var response = SendCommand(commandCode, 255);
            if (response < 0 || response > 100) {
                throw new IOException($"Invalid percent value {response}.");
            }
            return response;
        }

        void SetPercent(String commandCode, Byte value) {
            if (value < 0 || value > 100) {
                throw new IOException($"Invalid percent value {value}.");
            }
            SendCommand(commandCode, value);
        }

        public Byte Backlight { get => GetPercent("mg"); set => SetPercent("mg", value); }
        public Byte Balance { get => GetPercent("kt"); set => SetPercent("kt", value); }
        public Byte Bass { get => GetPercent("ks"); set => SetPercent("ks", value); }
        public Byte Brightness { get => GetPercent("kh"); set => SetPercent("kh", value); }
        public Byte Color { get => GetPercent("ki"); set => SetPercent("ki", value); }
        public Byte ColorTemperature { get => GetPercent("xu"); set => SetPercent("xu", value); }
        public Byte Contrast { get => GetPercent("kg"); set => SetPercent("kg", value); }
        public Byte Sharpness { get => GetPercent("kk"); set => SetPercent("kk", value); }
        public Byte Tint { get => GetPercent("kj"); set => SetPercent("kj", value); }
        public Byte Treble { get => GetPercent("kr"); set => SetPercent("kr", value); }
        public Byte Volume { get => GetPercent("kf"); set => SetPercent("kf", value); }

        Boolean GetBoolean(String commandCode) => SendCommand(commandCode, 255) == 1;
        void SetBoolean(String commandCode, Boolean value) => SendCommand(commandCode, (Byte)(value ? 1 : 0));

        public Boolean Mute { get => GetBoolean("ke"); set => SetBoolean("ke", value); }
        public Boolean OnScreenDisplay { get => GetBoolean("kl"); set => SetBoolean("kl", value); }
        public Boolean Power { get => GetBoolean("ka"); set => SetBoolean("ka", value); }
        public Boolean RemoteControlLock { get => GetBoolean("km"); set => SetBoolean("km", value); }

        Byte GetByte(String commandCode) => SendCommand(commandCode, 255);
        void SetByte(String commandCode, Byte value) => SendCommand(commandCode, value);

        public Byte AspectRatio { get => GetByte("kc"); set => SetByte("kc", value); }
        public Byte EnergySaving { get => GetByte("jq"); set => SendCommand("jq", value); }
        public Byte ImageStickingMinimizationMethod { get => GetByte("jp"); set => SetByte("jp", value); }
        public Byte Input { get => GetByte("xb"); set => SetByte("xb", value); }
        public Byte ScreenMute { get => GetByte("kd"); set => SetByte("kd", value); }

        public void Dispose() {
            var port = Interlocked.Exchange(ref _Port, null);
            port?.Dispose();
        }
    } 
}