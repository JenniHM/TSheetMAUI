using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;
using TSheetMAUI.Models;

namespace TSheetMAUI;

public partial class WorkAssignmentsPage : ContentPage
{
    int eId;
    string lat;
    string lon;

    public WorkAssignmentsPage(int id)
    {
        InitializeComponent();

        eId = id;

        
        //Latausilmoitukset
        lon_label.Text = "Sijaintia haetaan";
        wa_lataus.Text = "Noudetaan työtehtäviä...";

        GetCurrentLocation();

        //Sijainnin haku ja näyttäminen

        
        async void GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    lon_label.Text = "Longitude: " + Math.Round(location.Longitude, 2);
                    lat_label.Text = "Latitude: " + Math.Round(location.Latitude, 2);

                    lat = Math.Round(location.Latitude, 2).ToString();
                    lon = Math.Round(location.Longitude, 2).ToString();
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                await DisplayAlert("Virhe", fnsEx.ToString(), "OK");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                await DisplayAlert("Virhe", fneEx.ToString(), "OK");
            }
            catch (PermissionException pEx)
            {
                await DisplayAlert("Virhe", pEx.ToString(), "OK");
            }
            catch (Exception ex)
            {
            await DisplayAlert("Virhe", ex.ToString(), "OK");
            }
    }


    LoadDataFromRestAPI();

        async void LoadDataFromRestAPI()
        {
            try
            {
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri("https://tyonohjaus.azurewebsites.net");
                string json = await client.GetStringAsync("api/workassignments");

                IEnumerable<WorkAssignment> wa = JsonConvert.DeserializeObject<WorkAssignment[]>(json);

                ObservableCollection<WorkAssignment> dataa = new ObservableCollection<WorkAssignment>(wa);
                

                waList.ItemsSource = dataa;
                wa_lataus.Text = "";

            }

            catch (Exception e)
            {
                await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");

            }
        }
    }

    async void Aloitus_Clicked(object sender, EventArgs e)
    {

        WorkAssignment wa = (WorkAssignment)waList.SelectedItem;
        if(wa == null)
        {
            await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "OK");
            return;
        }
        try
        {
            Operation op = new Operation
            {
                EmployeeID = eId,
                WorkAssignmentID = wa.IdWorkAssignment,
                CustomerID = wa.IdCustomer,
                OperationType = "start",
                Comment = "Aloitettu",
                Latitude = lat,
                Longitude = lon
            };

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://tyonohjaus.azurewebsites.net");

            //Muutetaan em dataobjekti Jsoniksi
            string input = JsonConvert.SerializeObject(op);
            StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

            //Lähetetään serialisoitu objekti back-endiin Post-pyyntönä
            HttpResponseMessage mes = await client.PostAsync("/api/workassignments", content);

            //Otetaan vastaan palvelimen vastaus
            string reply = await mes.Content.ReadAsStringAsync();

            //Asetetaan vastaus serialisoituna success-muuttujaan
            bool success = JsonConvert.DeserializeObject<bool>(reply);

            if(success == false )
            {
                await DisplayAlert("Ei voida aloittaa", "Työ on jo käynnissä", "OK");
            }
            else if (success == true )
            {
                await DisplayAlert("Työ aloitettu", "Työ käynnissä", "OK");
            }
        }
        catch (Exception ex)
        {

            await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
        }
       
    }

    async void Lopetus_Clicked(object sender, EventArgs e)
    {

        WorkAssignment wa = (WorkAssignment)waList.SelectedItem;
        if (wa == null)
        {
            await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "OK");
            return;
        }

        string result = await DisplayPromptAsync("Kommentti", "Kirjoita kommentti");
        try
        {
            Operation op = new Operation
            {
                EmployeeID = eId,
                WorkAssignmentID = wa.IdWorkAssignment,
                CustomerID = wa.IdCustomer,
                OperationType = "stop",
                Comment = result,
                Latitude = lat,
                Longitude = lon
            };

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://tyonohjaus.azurewebsites.net");

            //Muutetaan em dataobjekti Jsoniksi
            string input = JsonConvert.SerializeObject(op);
            StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

            //Lähetetään serialisoitu objekti back-endiin Post-pyyntönä
            HttpResponseMessage mes = await client.PostAsync("/api/workassignments", content);

            //Otetaan vastaan palvelimen vastaus
            string reply = await mes.Content.ReadAsStringAsync();

            //Asetetaan vastaus serialisoituna success-muuttujaan
            bool success = JsonConvert.DeserializeObject<bool>(reply);

            if (success == false)
            {
                await DisplayAlert("Ei voida lopettaa", "Työtä ei ole aloitettu", "OK");
            }
            else if (success == true)
            {
                await DisplayAlert("Työn päättyminen", "Työ on päättynyt", "OK");

                await Navigation.PushAsync(new WorkAssignmentsPage(eId));
            }
        }
        catch (Exception ex)
        {

            await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
        }

    }
    

    async void Kuva_Clicked(object sender, EventArgs e)
    {
        WorkAssignment wa = (WorkAssignment)waList.SelectedItem;
        if (wa == null)
        {
            await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä", "OK");
            return;
        }
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using FileStream localFileStream = File.OpenWrite(localFilePath);

                    await sourceStream.CopyToAsync(localFileStream);
                }
               
            }
        }
        catch (Exception exc)
        {

            await DisplayAlert(exc.GetType().Name, exc.Message, "OK");
        }
    }
}