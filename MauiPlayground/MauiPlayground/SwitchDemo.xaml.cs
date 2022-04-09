namespace MauiPlayground;

public partial class SwitchDemo : ContentPage
{
	public SwitchDemo()
	{
		InitializeComponent();
		BindingContext = new ViewModel();
	}

    private void Toggle(object sender, EventArgs e)
    {
		var vm = (ViewModel)BindingContext;
		vm.Toggled = !vm.Toggled;
		manualLabel1.Text = $"Set in click handler: {switchControl.IsToggled}";
	}

	private void Toggled(object sender, ToggledEventArgs e) =>
		manualLabel2.Text = $"Set in toggled handler: {switchControl.IsToggled}";

	private class ViewModel : ViewModelBase
	{
		private bool toggled;
		public bool Toggled
		{
			get => toggled;
			set => SetProperty(ref toggled, value);
		}
	}
}