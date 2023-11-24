// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

/// <summary>
/// Workaround for https://codeblog.jonskeet.uk/2022/02/17/diagnosing-a-visca-camera-issue/
/// </summary>
public class TcpSendLock
{
    private readonly TimeSpan? postSendDelay;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

    /// <summary>
    /// Creates a new send lock which can be shared by multiple clients.
    /// </summary>
    /// <param name="postSendDelay">The length of time to wait (while still holding the lock)
    /// after sending each packet, or null for no delay.</param>
    public TcpSendLock(TimeSpan? postSendDelay)
    {
        this.postSendDelay = postSendDelay;
    }

    public Task AcquireAsync(CancellationToken cancellationToken) =>
        semaphore.WaitAsync(cancellationToken);

    public Task PostSendDelayAsync(CancellationToken cancellationToken) =>
        postSendDelay is TimeSpan delay
        ? Task.Delay(delay, cancellationToken)
        : Task.CompletedTask;

    public void Release() => semaphore.Release();
}
