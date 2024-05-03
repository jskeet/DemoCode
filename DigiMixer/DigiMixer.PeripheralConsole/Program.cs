// A small console application which allows peripherals to control
// a digital mixer, without any UI (which means it can be portable).

using Commons.Music.Midi;
using DigiMixer.Controls;
using DigiMixer.Core;
using DigiMixer.PeripheralConsole;
using JonSkeet.CoreAppUtil;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

if (args.Length != 1)
{
    Console.WriteLine("Arguments: <path to config file>.");
    var manager = MidiAccessManager.Default;
    Console.WriteLine("MIDI input devices");
    LogDevices(manager.Inputs);
    Console.WriteLine("MIDI output devices");
    LogDevices(manager.Outputs);

    void LogDevices(IEnumerable<IMidiPortDetails> devices)
    {
        foreach (var device in devices)
        {
            Console.WriteLine($"  Name: {device.Name}; ID: {device.Id}");
        }
    }
    return 1;
}

var config = JsonUtilities.LoadJson<DigiMixerAppConfig>(args[0]);
AsyncPump.Run(StartMixerController);

return 0;

async Task StartMixerController()
{
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(builder =>
    {
        builder.AddSimpleConsole(options =>
        {
            options.UseUtcTimestamp = true;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z '";
        });
        // TODO: Is this right?
        builder.SetMinimumLevel(config.Logging.LogLevel["Default"]);
        foreach (var filter in config.Logging.LogLevel)
        {
            builder.AddFilter(filter.Key, filter.Value);
        }
    });
    var serviceProvider = serviceCollection.BuildServiceProvider();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    var mixerVm = new DigiMixerViewModel(loggerFactory.CreateLogger("Mixer"), config.Mixer,
        // We don't use the meters, so let's not create unnecessary network traffic.
        new MixerApiOptions { MeterOptions = new() { UpdateFrequency = MeterUpdateFrequency.Off } });
    var peripheralController = PeripheralController.Create(loggerFactory, mixerVm, enablePeripherals: true);
    // Start the peripheral monitoring task. This is all we're really interested in.
    await peripheralController.Start();
}
