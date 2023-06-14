namespace TSheetMAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new NavigationPage(new EmployeePage());
	}
}
