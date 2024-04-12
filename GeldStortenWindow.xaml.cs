using System;
using System.Windows;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class GeldStortenWindow : Window
    {
        private string huidigRekeningnummer; // Variabele om het rekeningnummer bij te houden

        public GeldStortenWindow(string rekeningnummer)
        {
            InitializeComponent();
            huidigRekeningnummer = rekeningnummer;
        }

        private void StortenButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BedragTextBox.Text, out decimal bedrag))
            {
                if (bedrag <= 0)
                {
                    MessageBox.Show("Voer een positief bedrag in om te storten.");
                }
                else
                {
                    ProcesGeldStorting(bedrag);
                }
            }
            else
            {
                MessageBox.Show("Voer een geldig bedrag in.");
            }
        }

        private void ProcesGeldStorting(decimal bedrag)
        {
            string updateRekeningSaldoQuery = "UPDATE rekening SET Saldo = Saldo + @bedrag WHERE Rekeningnummer = @rekeningnummer";
            string insertTransactieQuery = "INSERT INTO transactie (datum, bedrag, Rekening_idRekening, Rekening_Gebruiker_idGebruiker) VALUES (@datum, @bedrag, (SELECT idRekening FROM rekening WHERE Rekeningnummer = @rekeningnummer), (SELECT idGebruiker FROM gebruiker WHERE Gebruikersnaam = (SELECT Gebruikersnaam FROM rekening WHERE Rekeningnummer = @rekeningnummer)))";

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Bedrag toevoegen aan saldo van rekening
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

                        MessageBox.Show($"Bedrag van {bedrag} is succesvol gestort op uw rekening.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Er is een fout opgetreden bij het storten van geld op uw rekening: {ex.Message}");
                    }
                }
            }
        }
    }
}
