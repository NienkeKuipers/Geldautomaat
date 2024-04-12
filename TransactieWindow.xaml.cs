using System;
using System.Collections.Generic;
using System.Windows;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class TransactieWindow : Window
    {
        private string huidigRekeningnummer;

        public TransactieWindow(string rekeningnummer)
        {
            InitializeComponent();
            huidigRekeningnummer = rekeningnummer;

            LaatsteTransactiesOphalenEnWeergeven();
        }

        private void LaatsteTransactiesOphalenEnWeergeven()
        {
            List<string> laatsteTransacties = GetLaatsteTransacties();

            foreach (var transactie in laatsteTransacties)
            {
                TransactiesListBox.Items.Add(transactie);
            }
        }

        private List<string> GetLaatsteTransacties()
        {
            List<string> transacties = new List<string>();

            string query = "SELECT Datum, Bedrag FROM transactie WHERE Rekening_Gebruiker_idGebruiker = (SELECT Gebruiker_idGebruiker FROM rekening WHERE Rekeningnummer = @rekeningnummer) ORDER BY Datum DESC LIMIT 3";


            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);

                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime datum = reader.GetDateTime(0);
                            decimal bedrag = reader.GetDecimal(1);

                            string transactie = $"{datum.ToString("dd-MM-yyyy")} - Bedrag: €{bedrag:F2}";

                            transacties.Add(transactie);
                        }
                    }
                }
            }

            return transacties;
        }
    }
}
