using CameraControl.Visca;
using Microsoft.Extensions.Logging;

// Simple console app to allow easy testing of cameras.

var loggingFactory = LoggerFactory.Create(builder => builder.AddConsole().AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss.FFFFFF";
}).SetMinimumLevel(LogLevel.Trace));
var logger = loggingFactory.CreateLogger("Visca");

// PTZOptics 
//var controller = ViscaController.ForTcp("192.168.1.45", 5678, ViscaMessageFormat.Raw, logger: logger);
//var controller = ViscaController.ForUdp("192.168.1.45", 1259, ViscaMessageFormat.Raw, logger: logger);

// Tail Air on Wifi
var controller = ViscaController.ForUdp("192.168.1.83", 52381, ViscaMessageFormat.Encapsulated, logger: logger);

var power = await controller.GetPowerStatus();
Console.WriteLine(power);
