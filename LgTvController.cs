using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LagrangianDesign.LgTvControl {
    [ApiController]
    [Route("[controller]")]
    public sealed class LgTvController : ControllerBase {
        readonly ILogger<LgTvController> Logger;
        readonly LgTvSerialPort LgTvSerialPort;
        public LgTvController(
            ILogger<LgTvController> logger,
            LgTvSerialPort lgTvSerialPort
            ) {
            (Logger, LgTvSerialPort) = (logger, lgTvSerialPort);
        }

        [HttpGet("Backlight")]
        public Int32 GetBacklight() => LgTvSerialPort.Backlight;
        [HttpPut("Backlight")]
        public void SetBacklight([FromBodyAttribute] Int32 value) {
            LgTvSerialPort.Backlight = value;
        } 
        [HttpGet("Input")]
        public Int32 GetInput() => LgTvSerialPort.Input;
        [HttpPut("Input")]
        public void SetInput([FromBodyAttribute] Int32 value) {
            LgTvSerialPort.Input = value;
        } 
        [HttpGet("OnScreenDisplay")]
        public Boolean GetOnScreenDisplay() => LgTvSerialPort.OnScreenDisplay;
        [HttpPut("OnScreenDisplay")]
        public void SetOnScreenDisplay([FromBodyAttribute] Boolean value) {
            LgTvSerialPort.OnScreenDisplay  = value;
        } 
        [HttpGet("Power")]
        public Boolean GetPower() => LgTvSerialPort.Power;
        [HttpPut("Power")]
        public void SetPower([FromBodyAttribute] Boolean value) {
            LgTvSerialPort.Power = value;
        } 
    }
}
