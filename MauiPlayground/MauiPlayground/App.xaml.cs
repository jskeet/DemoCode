namespace MauiPlayground;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new ListViewDemo();
    }
}
