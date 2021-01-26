// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System.Windows.Controls;
using System.Windows.Input;

namespace OscMixerControl.Wpf
{
    /// <summary>
    /// Scroll viewer that automatically scrolls at the bottom. (For logs etc.)
    /// </summary>
    public class BetterScrollViewer : ScrollViewer
    {
        private bool autoScroll = true;

        public bool AutoScrollAtBottom { get; set; }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);
            if (!AutoScrollAtBottom)
            {
                return;
            }
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                // Set to autoscroll iff the scroll bar is at the bottom.
                autoScroll = VerticalOffset == ScrollableHeight;
            }
            else if (autoScroll)
            {   // Content changed and auto-scroll mode set
                ScrollToVerticalOffset(ExtentHeight);
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
            ScrollToVerticalOffset(VerticalOffset - e.Delta / 10);
            e.Handled = true;
        }
    }
}
