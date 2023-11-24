// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CameraControl.Visca.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ViscaController controller;

        public event PropertyChangedEventHandler PropertyChanged;

        public int PanSpeed { get; private set; } = 0;
        public int TiltSpeed { get; private set; } = 0;
        public int ZoomSpeed { get; private set; } = 0;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            controller = ViscaController.ForTcp(ipAddress.Text, 5678, ViscaMessageFormat.Raw);
        }

        private void StopContinuousPanTilt(object sender, MouseButtonEventArgs e) => SetPanTiltSpeed(0, 0);
        private void StopContinuousZoom(object sender, MouseButtonEventArgs e) => SetZoomSpeed(0);

        private void AdjustContinuousPanTilt(object sender, MouseButtonEventArgs e) =>
            AdjustContinuousPanTilt(sender, (MouseEventArgs) e);

        private void AdjustContinuousPanTilt(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var position = e.GetPosition(panButton);

            int panSpeed = NormalizeSpeed(position.X, panButton.ActualWidth, ViscaController.MaxPanTiltSpeed);
            // Tilt has "up is positive" in camera coordinates, but "up is negative" in screen coordinates
            int tiltSpeed = -NormalizeSpeed(position.Y, panButton.ActualHeight, ViscaController.MaxPanTiltSpeed);
            SetPanTiltSpeed(panSpeed, tiltSpeed);
        }

        private void AdjustContinuousZoom(object sender, MouseButtonEventArgs e) =>
            AdjustContinuousZoom(sender, (MouseEventArgs) e);

        private void AdjustContinuousZoom(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var position = e.GetPosition(zoomButton);
            int zoomSpeed = NormalizeSpeed(position.X, zoomButton.ActualWidth, ViscaController.MaxZoomSpeed);
            SetZoomSpeed(zoomSpeed);
        }

        private void SetPanTiltSpeed(int panSpeed, int tiltSpeed)
        {
            if (panSpeed == PanSpeed && tiltSpeed == TiltSpeed)
            {
                return;
            }
            bool? panDirection = panSpeed == 0 ? default(bool?) : panSpeed > 0;
            bool? tiltDirection = tiltSpeed == 0 ? default(bool?) : tiltSpeed > 0;
            controller?.ContinuousPanTilt(panDirection, tiltDirection, AbsSpeed(panSpeed), AbsSpeed(tiltSpeed), default);
            PanSpeed = panSpeed;
            TiltSpeed = tiltSpeed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PanSpeed)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TiltSpeed)));
        }

        private void SetZoomSpeed(int zoomSpeed)
        {
            if (zoomSpeed == ZoomSpeed)
            {
                return;
            }
            bool? zoomDirection = zoomSpeed == 0 ? default(bool?) : zoomSpeed > 0;
            controller?.ContinuousZoom(zoomDirection, AbsSpeed(zoomSpeed), default);
            ZoomSpeed = zoomSpeed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomSpeed)));
        }

        /// <summary>
        /// Normalizes a speed to the range [-maxValue, maxValue], assuming that <paramref name="position"/>
        /// should be in the range [0, visualScale] (but might be outside that range, in which case
        /// the maximum value should be used).
        /// </summary>
        private static int NormalizeSpeed(double position, double visualScale, int maxValue)
        {
            // Scale to -1 / +1
            double scaledPosition = (position / visualScale) * 2 - 1;
            int speed = (int) (scaledPosition * (maxValue + 1));
            speed = Math.Min(speed, maxValue);
            speed = Math.Max(speed, -maxValue);
            return speed;
        }

        /// <summary>
        /// Works out the speed to use in the VISCA packet, assuming that a speed
        /// of 0 should become 1 (as other code will have set the direction to ignore the
        /// value) and negative values should become positive.
        /// </summary>
        private static byte AbsSpeed(int speed) => (byte) Math.Max(Math.Abs(speed), 1);

        private void Reset(object sender, RoutedEventArgs e) => controller?.GoHome();
    }
}
