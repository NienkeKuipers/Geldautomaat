using System.Windows;

namespace Geldautomaat
{
    public partial class DashboardWindow : Window
    {
        private string huidigeRekeningnummer;


        public DashboardWindow(string rekeningnummer)
        {
            InitializeComponent();
            huidigeRekeningnummer = rekeningnummer;
        }

        private void SaldoButton_Click(object sender, RoutedEventArgs e)
        {
            SaldoWindow saldoWindow = new SaldoWindow(huidigeRekeningnummer);
            saldoWindow.Show();
        }

        private void GeldOpnemenButton_Click(object sender, RoutedEventArgs e)
        {
            BedragOpnemenWindow bedragOpnemenWindow = new BedragOpnemenWindow(huidigeRekeningnummer);
            bedragOpnemenWindow.Show();
        }

        private void Transactie_Click(object sender, RoutedEventArgs e)
        {
            TransactieWindow transactiewindow = new TransactieWindow(huidigeRekeningnummer);
            transactiewindow.Show();
        }

        private void GeldStortenButton_Click(object sender, RoutedEventArgs e)
        {
            BedragStortenWindow bedragstortenwindow = new BedragStortenWindow(huidigeRekeningnummer);
            bedragstortenwindow.Show();
        }
    }
}
