using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class BedragStortenWindow : Window
    {
        private string huidigRekeningnummer;

        public BedragStortenWindow(string rekeningnummer)
        {
            InitializeComponent();
            huidigRekeningnummer = rekeningnummer;
        }

        private void BedragButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string content = button.Content.ToString().Replace(" euro", "");
            if (decimal.TryParse(content, out decimal bedrag))
            {
                ProcesGeldStorting(bedrag);
            }
        }

        private void AndereBedragButton_Click(object sender, RoutedEventArgs e)
        {
            GeldStortenWindow geldStortenWindow = new GeldStortenWindow(huidigRekeningnummer);
            geldStortenWindow.Show();
            this.Close();
        }

        private void AfbrekenButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboardWindow = new DashboardWindow(huidigRekeningnummer);
            dashboardWindow.Show();
            this.Close();
        }

        private void ProcesGeldStorting(decimal bedrag)
        {
            string updateRekeningSaldoQuery = "UPDATE rekening SET Saldo = Saldo + @bedrag WHERE Rekeningnummer = @rekeningnummer";
            string updateGebruikerSaldoQuery = "UPDATE gebruiker SET Saldo = Saldo + @bedrag WHERE idGebruiker = (SELECT Gebruiker_idGebruiker FROM rekening WHERE Rekeningnummer = @rekeningnummer)";
            string insertTransactieQuery = "INSERT INTO transactie (datum, bedrag, Rekening_idRekening, Rekening_Gebruiker_idGebruiker) VALUES (@datum, @bedrag, (SELECT idRekening FROM rekening WHERE Rekeningnummer = @rekeningnummer), (SELECT Gebruiker_idGebruiker FROM rekening WHERE Rekeningnummer = @rekeningnummer))";

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Bewerk saldo van rekening
                        using (MySqlCommand command = new MySqlCommand(updateRekeningSaldoQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@bedrag", bedrag);
                            command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);
                            command.ExecuteNonQuery();
                        }

                        // Bewerk saldo van gebruiker
                        using (MySqlCommand command = new MySqlCommand(updateGebruikerSaldoQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@bedrag", bedrag);
                            command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);
                            command.ExecuteNonQuery();
                        }

                        // Voeg transactiegegevens toe
                        using (MySqlCommand command = new MySqlCommand(insertTransactieQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@datum", DateTime.Now);
                            command.Parameters.AddWithValue("@bedrag", bedrag);
                            command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);
                            command.ExecuteNonQuery();
                        }

                        // Bevestig de transactie
                        transaction.Commit();

                        MessageBox.Show($"Bedrag van {bedrag} is succesvol gestort op uw rekening.");
                    }
                    catch (Exception ex)
                    {
                        // Rol de transactie terug bij fouten
                        transaction.Rollback();
                        MessageBox.Show($"Er is een fout opgetreden bij het storten van geld op uw rekening: {ex.Message}");
                    }
                }
            }
        }
    }
}