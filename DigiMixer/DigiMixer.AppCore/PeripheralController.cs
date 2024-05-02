using IconPlatform.Model;
using JonSkeet.CoreAppUtil;
using Microsoft.Extensions.Logging;
using XTouchMini.Model;

namespace DigiMixer.Controls;

public class PeripheralController : IAsyncDisposable
{
    private readonly XTouchDigiMixerController xtouchDigiMixerController;
    private readonly IconPlatformMixerController platformMMixerController;
    private readonly IconPlatformMixerController platformXMixerController;
    private readonly ILogger logger;

    private bool disposed;

    public StatusViewModel XTouchStatus { get; }
    public StatusViewModel PlatformMStatus { get; }
    public StatusViewModel PlatformXStatus { get; }

    private PeripheralController(ILogger logger,
        XTouchDigiMixerController xtouchDigiMixerController,
        IconPlatformMixerController platformMMixerController, IconPlatformMixerController platformXMixerController)
    {
        this.logger = logger;
        this.xtouchDigiMixerController = xtouchDigiMixerController;
        this.platformMMixerController = platformMMixerController;
        this.platformXMixerController = platformXMixerController;
        XTouchStatus = new StatusViewModel("X-Touch Mini");
        PlatformMStatus = new StatusViewModel("Platform-M");
        PlatformXStatus = new StatusViewModel("Platform-X");
    }

    public static PeripheralController Create(ILoggerFactory loggerFactory, DigiMixerViewModel mixerVm, bool enablePeripherals)
    {
        var config = mixerVm.Config;
        var xtouchController = enablePeripherals && !string.IsNullOrEmpty(config.XTouchMiniDevice)
            ? new XTouchMiniMackieController(CreateLogger("XTouchMini"), config.XTouchMiniDevice) : null;
        var platformMController = enablePeripherals && !string.IsNullOrEmpty(config.IconMPlusDevice)
            ? new PlatformMXController(CreateLogger("IconM"), config.IconMPlusDevice, PlatformMXController.PlatformMModelId)
            : null;
        var platformXController = enablePeripherals && !string.IsNullOrEmpty(config.IconXPlusDevice)
            ? new PlatformMXController(CreateLogger("IconX"), config.IconXPlusDevice, PlatformMXController.PlatformXModelId)
            : null;

        var xtouchMixerController = xtouchController is not null
            ? new XTouchDigiMixerController(CreateLogger("XTouchMixerController"), mixerVm, xtouchController, config.XTouchSensitivity, config.XTouchMainVolumeEnabled)
            : null;
        var platformMMixerController = platformMController is not null
            ? new IconPlatformMixerController(CreateLogger("PlatformMMixerController"), mixerVm, platformMController, 0, controlMain: true)
            : null;
        var platformXMixerController = platformXController is not null
            ? new IconPlatformMixerController(CreateLogger("PlatformXMixerController"), mixerVm, platformXController, 8, controlMain: false)
            : null;
        return new PeripheralController(CreateLogger("PeripheralController"), xtouchMixerController, platformMMixerController, platformXMixerController);

        ILogger CreateLogger(string name) => loggerFactory.CreateLogger(name);
    }

    public async Task Start()
    {
        await Task.Delay(500);

        while (!disposed)
        {
            try
            {
                await CheckControllerStatuses();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while checking status");
            }
            // Only a short delay here... this means we update the display more quickly
            // on changes, and we're less likely to run into an X-Touch "unplug/plug" issue
            // where we don't detect the disconnect.
            await Task.Delay(1000);
        }
    }

    public ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return ValueTask.CompletedTask;
        }
        disposed = true;
        return Disposables.DisposeAllAsyncWithCatch([xtouchDigiMixerController, platformMMixerController, platformXMixerController], logger);
    }

    private async Task CheckControllerStatuses()
    {
        await MaybeCheckStatus(xtouchDigiMixerController?.CheckConnectionAsync(), XTouchStatus);
        await MaybeCheckStatus(platformMMixerController?.CheckConnectionAsync(), PlatformMStatus);
        await MaybeCheckStatus(platformXMixerController?.CheckConnectionAsync(), PlatformXStatus);

        async Task MaybeCheckStatus(Task<bool> task, StatusViewModel status)
        {
            if (task is null)
            {
                status.ReportNormal("Not enabled");
                return;
            }
            // We never report a warning here - it's fine for these to be disconnected.
            status.ReportNormal(await task ? "Connected" : "Disconnected");
        }
    }
}
