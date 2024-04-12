using System;
using System.Windows;
using geldautomaat.Controllers;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class MainWindow : Window
    {
        private SQL database;
        private MySqlConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            database = new SQL();
            connection = database.Connection;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string rekeningnummer = GebruikersnaamTextBox.Text;
            string pincode = PincodeTextBox.Password;

            // Query om het wachtwoord op te halen op van het rekeningnummer
            string wachtwoordQuery = "SELECT Wachtwoord FROM gebruiker u JOIN rekening r ON u.idGebruiker = r.Gebruiker_idGebruiker WHERE r.Rekeningnummer = @rekeningnummer";

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                using (MySqlCommand command = new MySqlCommand(wachtwoordQuery, connection))
                {
                    command.Parameters.AddWithValue("@rekeningnummer", rekeningnummer);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string correctWachtwoord = reader.GetString("Wachtwoord");

                            // Controleer of het ingevoerde wachtwoord overeenkomt met het wachtwoord in de database
                            if (pincode == correctWachtwoord)
                            {
                                DashboardWindow dashboardWindow = new DashboardWindow(rekeningnummer); // Stuur het rekeningnummer naar het dashboardvenster
                                dashboardWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Ongeldig wachtwoord. Probeer opnieuw.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Rekeningnummer niet gevonden. Probeer opnieuw.");
                        }
                    }
                }
            }
        }
    }
}
