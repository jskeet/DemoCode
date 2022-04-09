namespace MauiPlayground;

public partial class ListViewDemo : ContentPage
{
	public ListViewDemo()
	{
		InitializeComponent();
		BindingContext = new ViewModel();
	}

	private void Toggle(object sender, EventArgs e)
	{
		var vm = (ViewModel)BindingContext;
		vm.SelectedItem = vm.SelectedItem == "First" ? "Second" : "First";
		manualLabel1.Text = $"Set in click handler: {listView.SelectedItem}";
	}

	private void ItemSelected(object sender, EventArgs e) =>
		manualLabel2.Text = $"Set in item selected handler: {listView.SelectedItem}";

	private class ViewModel : ViewModelBase
	{
		public List<string> Items { get; } = new List<string> { "First", "Second" };

		private string selectedItem = "First";
		public string SelectedItem
		{
			get => selectedItem;
			set => SetProperty(ref selectedItem, value);
		}
	}
}