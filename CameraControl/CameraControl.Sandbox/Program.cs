using CameraControl.Visca;

// Simple console app to allow easy testing of cameras.

//var controller = ViscaController.ForTcp("192.168.1.45", 5678, ViscaMessageFormat.Raw);
//var controller = ViscaController.ForUdp("192.168.1.45", 1259, ViscaMessageFormat.Raw);
var controller = ViscaController.ForUdp("192.168.1.83", 52381, ViscaMessageFormat.Encapsulated);

var power = await controller.GetPowerStatus();
Console.WriteLine(power);
