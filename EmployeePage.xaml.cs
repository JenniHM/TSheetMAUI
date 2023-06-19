using System.Collections.ObjectModel;
using TSheetMAUI.Models;
using Newtonsoft.Json;

namespace TSheetMAUI;


public partial class EmployeePage : ContentPage
{
    //Alustetaan tiedostopolku ja sen sisältö
    string fileName = Path.Combine(FileSystem.AppDataDirectory, "tyontekija.txt");
    string text = "";
    

    // Työntekijä-listan alustaminen
    ObservableCollection<Employee> dataa = new ObservableCollection<Employee>();
    public EmployeePage()
	{
		InitializeComponent();

       
        bool doesExist = File.Exists(fileName);

        //Tarkistetaan onko työntekijää tallennettu sovellukseen
        if (doesExist == true)
        {
            text = File.ReadAllText(fileName);
            if (text.Length > 0)
            {
                employeeList.ItemsSource = text;
                tekijaLabel.Text = text;
                tekijaLabel.TextColor = Colors.DarkMagenta;
                delbutton.IsVisible = true;
                savebutton.IsVisible = false;
                
            }
            else
            {
                tekijaLabel.Text = "Haluatko sovelluksen muistavan valintasi?";
                savebutton.IsVisible = true;
            }
        }
        else
        {
            tekijaLabel.Text = "Valintaa ei ole tallennettu";
        }


        //Annetaan latausilmoitus
        emp_lataus.Text = "Ladataan työntekijöitä...";

        //Metodin kutsu
        LoadDataFromRestAPI();

        async void LoadDataFromRestAPI()
        {
            try
            {
               HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://tyonohjaus.azurewebsites.net");
                string json = await client.GetStringAsync("api/employees");

                IEnumerable<Employee> employees = JsonConvert.DeserializeObject<Employee[]>(json);
                // dataa -niminen observableCollection on alustettukin jo ylhäällä päätasolla että hakutoiminto pääsee siihen käsiksi.
                // Asetetaan sen sisältö ensi kerran tässä pienellä kepulikonstilla:
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

    //Siirtyminen työtehtävät-sivulle
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
            await Navigation.PushAsync(new WorkAssignmentsPage(id)); //Navigoidaan työtehtävät-sivulle
        }
    }

    //Työntekijälistan uudelleenlataus
    async void päivitysButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EmployeePage());
    }

    //Työntekijän tallentaminen paikalliseen tekstitiedostoon
    private void savebutton_Clicked(object sender, EventArgs e)
    {
        Employee emp = (Employee)employeeList.SelectedItem;
        if (emp == null)
        {
            DisplayAlert("Valinta puuttuu", "Valitse työntekijä", "OK");
            return;
        }
        else
        {
            text = emp.FirstName + " " + emp.LastName;
            File.WriteAllText(fileName, text);
            tekijaLabel.Text = text;
            tekijaLabel.TextColor = Colors.DarkMagenta;
            savebutton.IsVisible = false;
            delbutton.IsVisible = true;
        }
    }

    //Tallennetun työntekijän poistaminen tiedostosta, "unohtaminen"
    private void delbutton_Clicked(object sender, EventArgs e)
    {
        File.WriteAllText(fileName, "");
        text = "";
        tekijaLabel.Text = "";
        delbutton.IsVisible = false;
        savebutton.IsVisible = true;
    }
}




