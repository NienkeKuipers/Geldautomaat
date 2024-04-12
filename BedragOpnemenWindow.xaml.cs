using System;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Geldautomaat
{
    public partial class BedragOpnemenWindow : Window
    {
        private string huidigRekeningnummer;

        public BedragOpnemenWindow(string rekeningnummer)
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
                ProcesGeldOpname(bedrag);
            }
        }

        private void AndereBedragButton_Click(object sender, RoutedEventArgs e)
        {
            GeldOpnemenWindow geldOpnemenWindow = new GeldOpnemenWindow(huidigRekeningnummer);
            geldOpnemenWindow.Show();
            this.Close();
        }

        private void AfbrekenButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboardWindow = new DashboardWindow(huidigRekeningnummer);
            dashboardWindow.Show();
            this.Close();
        }

        private void ProcesGeldOpname(decimal bedrag)
        {
            string updateGebruikerSaldoQuery = "UPDATE gebruiker SET Saldo = Saldo - @bedrag WHERE idGebruiker = (SELECT Gebruiker_idGebruiker FROM rekening WHERE Rekeningnummer = @rekeningnummer)";
            string updateRekeningSaldoQuery = "UPDATE rekening SET Saldo = Saldo - @bedrag WHERE Rekeningnummer = @rekeningnummer";
            string insertTransactieQuery = "INSERT INTO transactie (datum, bedrag, Rekening_idRekening, Rekening_Gebruiker_idGebruiker) VALUES (@datum, @bedrag, (SELECT idRekening FROM rekening WHERE Rekeningnummer = @rekeningnummer), (SELECT idGebruiker FROM gebruiker WHERE idGebruiker = (SELECT Gebruiker_idGebruiker FROM rekening WHERE Rekeningnummer = @rekeningnummer)))";

            using (MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=root;Password=;Database=geldautomaat"))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Bedrag aftrekken van saldo van gebruiker
                        using (MySqlCommand command = new MySqlCommand(updateGebruikerSaldoQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@bedrag", bedrag);
                            command.Parameters.AddWithValue("@rekeningnummer", huidigRekeningnummer);
                            command.ExecuteNonQuery();
                        }

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

                        MessageBox.Show($"Bedrag van {bedrag} is succesvol opgenomen van uw rekening.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Er is een fout opgetreden bij het opnemen van geld van uw rekening: {ex.Message}");
                    }
                }
            }
        }
    }
}
