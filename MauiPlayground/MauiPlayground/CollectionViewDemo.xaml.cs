namespace MauiPlayground;

public partial class CollectionViewDemo : ContentPage
{
	public CollectionViewDemo()
	{
		InitializeComponent();
		BindingContext = new ViewModel();
	}

	private void Toggle(object sender, EventArgs e)
	{
		var vm = (ViewModel)BindingContext;
		vm.SelectedItem = vm.SelectedItem == "First" ? "Second" : "First";
		manualLabel.Text = $"Set in click handler: {collectionView.SelectedItem}";
	}

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