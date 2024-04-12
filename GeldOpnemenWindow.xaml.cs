using System;
using System.Windows;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class GeldOpnemenWindow : Window
    {
        private string huidigRekeningnummer; // Variabele om het rekeningnummer bij houden

        // Ontvangt het rekeningnummer als parameter
        public GeldOpnemenWindow(string rekeningnummer)
        {
            InitializeComponent();
            huidigRekeningnummer = rekeningnummer;
        }

        private void OpnemenButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BedragTextBox.Text, out decimal bedrag))
            {
                if (bedrag <= 0)
                {
                    MessageBox.Show("Voer een positief bedrag in om op te nemen.");
                }
                else
                {
                    ProcesGeldOpname(bedrag);
                }
            }
            else
            {
                MessageBox.Show("Voer een geldig bedrag in.");
            }
        }

        private void ProcesGeldOpname(decimal bedrag)
        {
            string updateRekeningSaldoQuery = "UPDATE rekening SET Saldo = Saldo - @bedrag WHERE Rekeningnummer = @rekeningnummer";
            string insertTransactieQuery = "INSERT INTO transactie (datum, bedrag, Rekening_idRekening, Rekening_Gebruiker_idGebruiker) VALUES (@datum, @bedrag, (SELECT idRekening FROM rekening WHERE Rekeningnummer = @rekeningnummer), (SELECT Gebruiker_idGebruiker FROM rekening WHERE Rekeningnummer = @rekeningnummer))";

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Bedrag aftrekken van saldo van rekening
                        using (MySqlCommand command = new MySqlCommand(updateRekeningSaldoQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@bedrag", bedrag);
                            command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);
                            command.ExecuteNonQuery();
                        }

                        // Toevoegen van transactie
                        using (MySqlCommand command = new MySqlCommand(insertTransactieQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@datum", DateTime.Now);
                            command.Parameters.AddWithValue("@bedrag", bedrag);
                            command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        MessageBox.Show($"Bedrag van {bedrag} is succesvol afgeschreven van uw rekening.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Er is een fout opgetreden bij het afschrijven van geld van uw rekening: {ex.Message}");
                    }
                }
            }
        }
    }
}
