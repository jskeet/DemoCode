// Copyright 2023 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace DigiMixer.Ssc;

/// <summary>
/// Interface for SscClient allowing for simple faking.
/// </summary>
public interface ISscClient : IDisposable
{
    /// <summary>
    /// Starts the client, connecting to the server.
    /// If this method has been called, the client should be disposed after use.
    /// </summary>
    void Start();

    /// <summary>
    /// Event raised when a message is received from the server.
    /// </summary>
    event EventHandler<SscMessage>? MessageReceived;

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">A cancellation token for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendMessage(SscMessage message, CancellationToken cancellationToken);
}
