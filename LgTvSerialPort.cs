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
        public LgTvSerialPort(String name, Int32 setId) {
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

        Int32 SendCommand(String commandCode, Int32 data) {
            if (commandCode?.Length != 2) {
                throw new ArgumentException($"Invalid command code {commandCode.Quoted()}.", nameof(commandCode));
            }
            if (data < 0 || data > 255) {
                throw new ArgumentOutOfRangeException(nameof(data));
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
                    && Int32.TryParse(response[7..^0], HexNumber, CultureInfo.InvariantCulture, out var result)) {
                    return result;
                }
                throw new IOException($"Response {response.Quoted()} to command {command[0..^1].Quoted()} not OK.");
            }
        }

        void SetBoolean(String commandCode, Boolean value)
            => SendCommand(commandCode, value ? 1 : 0);
        
        Boolean GetBoolean(String commandCode) => SendCommand(commandCode, 255) == 1;

        void SetPercent(String commandCode, Int32 value) {
            if (value < 0 || value > 100) {
                throw new IOException($"Invalid percent value {value}.");
            }
            SendCommand(commandCode, value);
        }
        Int32 GetPercent(String commandCode) {
            var response = SendCommand(commandCode, 255);
            if (response < 0 || response > 100) {
                throw new IOException($"Invalid percent value {response}.");
            }
            return response;
        }


        public Int32 Input {
            get => SendCommand("xb", 255);
            set => SendCommand("xb", value);
        }

        public Boolean Power {
            get => GetBoolean("ka");
            set => SetBoolean("ka", value);
        }

        public Boolean OnScreenDisplay {
            get => GetBoolean("kl");
            set => SetBoolean("kl", value);
        }

        public Int32 Backlight {
            get => GetPercent("mg");
            set => SetPercent("mg", value);
        }

        public void Dispose() {
            var port = Interlocked.Exchange(ref _Port, null);
            port?.Dispose();
        }
    } 
}