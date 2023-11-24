// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

namespace CameraControl.Visca;

internal interface IViscaClient : IDisposable
{
    /// <summary>
    /// Sends a single message to the VISCA endpoint,
    /// returning the final "done" response message.
    /// </summary>
    /// <param name="request">The message to send to the endpoint.</param>
    /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ViscaProtocolException">The VISCA endpoint violated the VISCA protocol.</exception>
    /// <exception cref="ViscaResponseException">The VISCA endpoint reported an error in its response.</exception>
    Task<ViscaMessage> SendAsync(ViscaMessage request, CancellationToken cancellationToken);
}
