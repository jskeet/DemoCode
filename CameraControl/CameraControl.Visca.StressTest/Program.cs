// Copyright 2022 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using CameraControl.Visca;

// This is a stress test hammering a camera from multiple threads (tasks).
// Motivation: in practice, we're seeing a lot of errors reported via VISCA, like this:
//   Error reported by VISCA interface. Error data: 90-60-02
// If we can provoke this, we may be able to fix it.

// Configuration
// PTZOptics camera
string host = "192.168.1.45";
int port = 5678;

// Minrray camera
// string host = "192.168.1.47";
// int port = 1259;

// True to use one controller (and therefore one TcpClient) for all tasks; false to use one per task
bool singleController = false;
int taskCount = 10;
TimeSpan delay = TimeSpan.FromMilliseconds(5);
TimeSpan commandTimeout = TimeSpan.FromSeconds(1);
TimeSpan testDuration = TimeSpan.FromMinutes(1);


// Main code. All the semaphores are just to so that we can get as much genuine concurrency
// as possible; tasks don't start up immediately, as the thread pool gradually adds tasks.
// So we have one set of semaphores to let the tasks each say "I'm ready" - and then a separate
// semaphore that the "controlling" code releases to say "Go!".
//
// It's entirely possible that using dedicated threads would be simpler here, but the library
// is async so we'd end up using the thread pool anyway (implicitly) unless we created dedicated
// synchronization contexts.
var readySemaphores = Enumerable.Range(1, taskCount).Select(_ => new Semaphore(0, 1)).ToList();
var goSemaphore = new Semaphore(0, taskCount);
DateTime testEnd = DateTime.UtcNow + testDuration;

// If we're going to use the same controller for all tasks, create it now.
var singletonController = singleController ? ViscaController.ForTcp(host, port, commandTimeout) : null;
// Start the tasks
var tasks = readySemaphores.Select(readySemaphore => RunTestAsync(readySemaphore)).ToList();

// 
Console.WriteLine("Waiting for tasks to be ready...");
foreach (var semaphore in readySemaphores)
{
    semaphore.WaitOne();
}
Console.WriteLine("Releasing tasks");
goSemaphore.Release(taskCount);

// Report results
await Task.WhenAll(tasks);
for (int i = 0; i < tasks.Count; i++)
{
    var (success, errorResponses, protocolErrors, cancellations) = tasks[i].Result;
    Console.WriteLine($"Task {i}: {success} / {errorResponses} / {protocolErrors} / {cancellations}");
}

// The actual test code.
async Task<(int, int, int, int)> RunTestAsync(Semaphore readySemaphore)
{
    // Effectively "switch to the thread pool".
    await Task.Yield();

    // Only start the test when all tasks are ready to go.
    readySemaphore.Release();
    goSemaphore.WaitOne();

    int success = 0;
    int errorResponses = 0;
    int protocolErrors = 0;
    int cancellations = 0;
    var controller = singletonController ?? ViscaController.ForTcp(host, port, commandTimeout);
    while (DateTime.UtcNow < testEnd)
    {
        try
        {
            await controller.GetPowerStatus();
            success++;
        }
        catch (ViscaResponseException)
        {
            errorResponses++;
        }
        catch (ViscaProtocolException)
        {
            protocolErrors++;
        }
        catch (OperationCanceledException)
        {
            cancellations++;
        }
        await Task.Delay(delay);
    }
    return(success, errorResponses, protocolErrors, cancellations);
}
