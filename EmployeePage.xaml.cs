using System.Collections.ObjectModel;
using TSheetMAUI.Models;
using Newtonsoft.Json;

namespace TSheetMAUI;


public partial class EmployeePage : ContentPage
{
    // Muuttujan alustaminen
    ObservableCollection<Employee> dataa = new ObservableCollection<Employee>();
    public EmployeePage()
	{
		InitializeComponent();

        //Annetaan latausilmoitus
        emp_lataus.Text = "Ladataan työntekijöitä...";

        LoadDataFromRestAPI();

        async void LoadDataFromRestAPI()
        {
            try
            {
               HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://tyonohjaus.azurewebsites.net");
                string json = await client.GetStringAsync("api/employees");

                IEnumerable<Employee> employees = JsonConvert.DeserializeObject<Employee[]>(json);
                // dataa -niminen observableCollection on alustettukin jo ylhäällä päätasolla että hakutoiminto,
                // pääsee siihen käsiksi.
                // asetetaan sen sisältö ensi kerran tässä pienellä kepulikonstilla:
                ObservableCollection<Employee> dataa2 = new ObservableCollection<Employee>(employees);
                dataa = dataa2;

                // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                employeeList.ItemsSource = dataa;

                // Tyhjennetään latausilmoitus label
                emp_lataus.Text = "";

            }
            catch (Exception e)
            {
                await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");
            }
        }
    }
    //Hakutoiminto
    private void OnSearchBarButtonPressed(object sender, EventArgs args)
    {
        SearchBar searchBar = (SearchBar)sender;
        string searchText = searchBar.Text;

        employeeList.ItemsSource = dataa.Where(x => x.LastName.ToLower().Contains(searchText.ToLower())
            || x.FirstName.ToLower().Contains(searchText.ToLower()));

    }
    async void navbutton_Clicked(object sender, EventArgs e)
    {
        Employee emp = (Employee)employeeList.SelectedItem;

        if(emp == null)
        {
            await DisplayAlert("Valinta puuttuu", "Valitse työntekijä", "OK"); //otsikko, teksti, kuittausnapin teksti
            return;
        }
        else
        {
            int id = emp.IdEmployee;
            await Navigation.PushAsync(new WorkAssignmentsPage(id)); //Navigoidaan uudelle sivulle
        }
    }

    async void päivitysButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EmployeePage());
    }
}




