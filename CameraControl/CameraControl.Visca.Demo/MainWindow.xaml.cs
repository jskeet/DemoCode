// Copyright 2020 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Windows;
using System.Windows.Input;

namespace CameraControl.Visca.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViscaController controller;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            controller = new ViscaController(ipAddress.Text, 5678);
            var rtspStream = WebEye.Stream.FromUri(
                uri: new Uri(rtspUrl.Text),
                connectionTimeout: TimeSpan.FromSeconds(10),
                streamTimeout: TimeSpan.FromSeconds(10),
                transport: WebEye.RtspTransport.Undefined,
                flags: WebEye.RtspFlags.PreferTcp);
            rtspStream.Start();
            cameraStream.Stream = rtspStream;
        }

        // All of the controller calls are asynchronous, but we're effectively using them in a fire and forget manner
        // anyway. There's no point in awaiting the task, as we don't do anything after it.

        private void ContinuousUpLeft(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(false, false);
        private void ContinuousUp(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(null, false);
        private void ContinuousUpRight(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(true, false);
        private void ContinuousLeft(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(false, null);
        private void ContinuousRight(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(true, null);
        private void ContinuousDownLeft(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(false, true);
        private void ContinuousDown(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(null, true);
        private void ContinuousDownRight(object sender, MouseButtonEventArgs e) => ContinuousPanTilt(true, true);
        private void StopContinuousPanTilt(object sender, MouseButtonEventArgs e) => controller?.ContinuousPanTilt(null, null, 1, 1, default);

        private void ContinuousPanTilt(bool? pan, bool? tilt)
        {
            var speed = (byte) panTiltSpeed.Value;
            controller?.ContinuousPanTilt(pan, tilt, speed, speed, default);
        }

        private void ContinuousZoomIn(object sender, MouseButtonEventArgs e) => controller?.ContinuousZoom(true, (byte) zoomSpeed.Value, default);
        private void ContinuousZoomOut(object sender, MouseButtonEventArgs e) => controller?.ContinuousZoom(false, (byte) zoomSpeed.Value, default);
        private void StopContinuousZoom(object sender, MouseButtonEventArgs e) => controller?.ContinuousZoom(null, 1, default);

        private void MoveToHome(object sender, RoutedEventArgs e) => controller?.GoHome(default);
    }
}
