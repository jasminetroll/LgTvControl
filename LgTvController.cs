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
        // GET and PUT methods for all public properties of LgTvSerialPort
        // are inserted here before this class is compiled (at runtime).
    }
}
