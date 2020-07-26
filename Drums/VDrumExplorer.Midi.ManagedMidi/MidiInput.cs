// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using Commons.Music.Midi;
using System;
using System.Linq;
using VDrumExplorer.Model.Midi;

namespace VDrumExplorer.Midi.ManagedMidi
{
    /// <summary>
    /// Implementation of <see cref="IMidiManager"/> using managed-midi.
    /// </summary>
    internal class MidiInput : Model.Midi.IMidiInput
    {
        private bool disposed = false;
        private readonly Commons.Music.Midi.IMidiInput managedInput;

        public event EventHandler<Model.Midi.MidiMessage>? MessageReceived;

        internal MidiInput(Commons.Music.Midi.IMidiInput managedInput)
        {
            this.managedInput = managedInput;
            managedInput.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MidiReceivedEventArgs args)
        {
            var data = args.Length == args.Data.Length && args.Start == 0
                ? args.Data : args.Data.Skip(args.Start).Take(args.Length).ToArray();
            var message = new Model.Midi.MidiMessage(data, args.Timestamp);
            MessageReceived?.Invoke(this, message);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            
            // Calling CloseAsync is significantly faster than calling Dispose.
            // This is slightly odd, as the implementation for desktop seems to call
            // CloseAsync too. Not sure what's going on, and maybe we should be waiting
            // the task to complete... but it doesn't seem to cause any harm.
            managedInput.CloseAsync();
        }
    }
}
