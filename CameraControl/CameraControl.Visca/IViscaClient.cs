// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CameraControl.Visca
{
    internal interface IViscaClient : IDisposable
    {
        /// <summary>
        /// Sends a single packet to the VISCA endpoint,
        /// returning the final "done" response packet.
        /// </summary>
        /// <param name="packet">The packet to send to the endpoint.</param>
        /// <param name="cancellationToken">A token to indicate that the method has been cancelled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ViscaProtocolException">The VISCA endpoint violated the VISCA protocol.</exception>
        /// <exception cref="ViscaResponseException">The VISCA endpoint reported an error in its response.</exception>
        Task<ViscaPacket> SendAsync(ViscaPacket request, CancellationToken cancellationToken);
    }
}
