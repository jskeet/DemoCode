// A small console application which allows peripherals to control
// a digital mixer, without any UI (which means it can be portable).

using Commons.Music.Midi;
using DigiMixer.Controls;
using DigiMixer.PeripheralConsole;
using JonSkeet.CoreAppUtil;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

if (args.Length != 1)
{
    Console.WriteLine("Arguments: <path to config file>");
    return 1;
}

var config = JsonUtilities.LoadJson<DigiMixerAppConfig>(args[0]);
AsyncPump.Run(StartMixerController);

return 0;

async Task StartMixerController()
{
    var manager = MidiAccessManager.Default;
    var inputPort = await manager.OpenInputAsync("24_0").ConfigureAwait(false);
    inputPort.MessageReceived += InputPort_MessageReceived;

    /*
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
    LogMidiDevices(loggerFactory.CreateLogger("Midi"));

    var mixerVm = new DigiMixerViewModel(loggerFactory.CreateLogger("Mixer"), config.Mixer);
    var peripheralController = PeripheralController.Create(loggerFactory, mixerVm, config.EnablePeripherals);
    // Start the peripheral monitoring task. This is all we're really interested in.
    await peripheralController.Start();
    */
}

void InputPort_MessageReceived(object sender, MidiReceivedEventArgs e)
{
    Console.WriteLine(BitConverter.ToString(e.Data));
}
/*
void LogMidiDevices(ILogger logger)
{
    var manager = MidiAccessManager.Default;
    logger.LogInformation("MIDI input devices");
    LogDevices(manager.Inputs);
    logger.LogInformation("MIDI output devices");
    LogDevices(manager.Outputs);

    void LogDevices(IEnumerable<IMidiPortDetails> devices)
    {
        foreach (var device in devices)
        {
            logger.LogInformation("  Name: {name}; ID: {id}", device.Name, device.Id);
        }
    }
}*/