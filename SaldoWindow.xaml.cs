using System;
using System.Windows;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class SaldoWindow : Window
    {
        private string huidigeRekeningnummer;

        public SaldoWindow(string rekeningnummer)
        {
            InitializeComponent();

            huidigeRekeningnummer = rekeningnummer;

            string query = "SELECT Saldo FROM rekening WHERE Rekeningnummer = @rekeningnummer";

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@rekeningnummer", rekeningnummer);

                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        SaldoTextBlock.Text = result.ToString();
                    }
                    else
                    {
                        SaldoTextBlock.Text = "Saldo niet beschikbaar";
                    }
                }
            }
        }
    }
}
