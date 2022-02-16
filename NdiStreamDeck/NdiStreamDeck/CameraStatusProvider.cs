using NewTek.NDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NdiStreamDeck
{
    /// <summary>
    /// Watches a <see cref="VideoFrameReceiver"/> to keep track of
    /// the number of frames received over time.
    /// </summary>
    internal class CameraStatusProvider : IDisposable
    {
        private readonly VideoFrameReceiver receiver;
        private int frames;
        private int lastWidth;
        private int lastHeight;
        private DateTime lastStatusCheck;

        internal CameraStatusProvider(VideoFrameReceiver receiver)
        {
            this.receiver = receiver;
            receiver.FrameReceived += HandleFrameReceived;
        }

        /// <summary>
        /// Generates the current status summary for this camera, including frame size and recent FPS.
        /// This resets the internal values.
        /// </summary>
        internal string GetStatus()
        {
            var now = DateTime.UtcNow;
            // Avoid division by 0 or negative values
            var secondsSinceLastUpdate = Math.Max((now - lastStatusCheck).TotalSeconds, 0.001);
            int fps = (int) (frames / secondsSinceLastUpdate);
            lastStatusCheck = now;
            string status = $"{receiver.SourceName}: {lastWidth}x{lastHeight} at {fps}fps";
            frames = 0;
            lastWidth = 0;
            lastHeight = 0;
            return status;
        }

        /// <summary>
        /// Updates the statistics when a frame is received - doesn't actually draw anything.
        /// </summary>
        private void HandleFrameReceived(object sender, VideoFrame frame)
        {
            frames++;
            lastWidth = frame.Width;
            lastHeight = frame.Height;
        }

        public void Dispose() => receiver.FrameReceived -= HandleFrameReceived;
    }
}
