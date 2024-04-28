// A small console application which allows peripherals to control
// a digital mixer, without any UI (which means it can be portable).

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
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging(builder =>
    {
        builder.AddSimpleConsole(options =>
        {
            options.UseUtcTimestamp = true;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";
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

    var mixerVm = new DigiMixerViewModel(loggerFactory.CreateLogger("Mixer"), config.Mixer);
    var peripheralController = PeripheralController.Create(loggerFactory, mixerVm, config.EnablePeripherals);
    // Start the peripheral monitoring task. This is all we're really interested in.
    await peripheralController.Start();
}
