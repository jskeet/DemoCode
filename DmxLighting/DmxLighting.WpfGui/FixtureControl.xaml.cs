// Copyright 2021 Jon Skeet. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using DmxLighting.Data;
using System.Windows.Controls;

namespace DmxLighting.WpfGui
{
    /// <summary>
    /// Interaction logic for FixtureControl.xaml
    /// </summary>
    public partial class FixtureControl : UserControl
    {
        private FixtureViewModel ViewModel => (FixtureViewModel) DataContext;

        public FixtureControl()
        {
            InitializeComponent();
        }

        private void ForceSend(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = ViewModel;
            if (vm is null)
            {
                return;
            }
            vm.Sender.SendUniverse(vm.Universe);
        }
    }
}
