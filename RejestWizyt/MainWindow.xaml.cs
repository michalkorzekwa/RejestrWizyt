using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace RejestWizyt
{
    public partial class MainWindow : Window
    {
        private WizytyContext _context;
        private System.Windows.Threading.DispatcherTimer _refreshTimer;
        private DateTime _lastRefresh = DateTime.Now;
        private int _lastRecordCount = 0;
        private string _currentConnectionString;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            InitializeDatabase();
            LoadSettingsToUI();
            SetupAutoRefresh();
        }

        private void LoadSettings()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var serverIp = config.AppSettings.Settings["ServerIP"]?.Value ?? "localhost";
            var databaseName = config.AppSettings.Settings["DatabaseName"]?.Value ?? "RecepcjaFirma";
            var instanceName = config.AppSettings.Settings["InstanceName"]?.Value ?? "SQLEXPRESS";

            _currentConnectionString = $"Server={serverIp}\\{instanceName};Database={databaseName};Trusted_Connection=True;Encrypt=False;";
        }

        private void LoadSettingsToUI()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ServerIpTextBox.Text = config.AppSettings.Settings["ServerIP"]?.Value ?? "localhost";
            DatabaseNameTextBox.Text = config.AppSettings.Settings["DatabaseName"]?.Value ?? "RecepcjaFirma";
            InstanceNameTextBox.Text = config.AppSettings.Settings["InstanceName"]?.Value ?? "SQLEXPRESS";

            ConnectionInfoTextBlock.Text = $"Aktualny connection string:\n{_currentConnectionString}";
        }

        private void InitializeDatabase()
        {
            try
            {
                _context?.Dispose(); // Dispose old context if exists
                _context = new WizytyContext(_currentConnectionString);
                _context.Database.EnsureCreated();
                OdswiezDane();
                ConnectionStatusTextBlock.Text = "Status: Połączono pomyślnie";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                _context?.Dispose();
                _context = null;
                ConnectionStatusTextBlock.Text = $"Status: Błąd połączenia - {ex.Message}";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Błąd połączenia z bazą danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupAutoRefresh()
        {
            _refreshTimer?.Stop(); // Stop existing timer
            _refreshTimer = new System.Windows.Threading.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(15);
            _refreshTimer.Tick += AutoRefresh_Tick;
            _refreshTimer.Start();
        }

        private void AutoRefresh_Tick(object sender, EventArgs e)
        {
            // Skip refresh if context is null or disposed
            if (_context == null) return;

            try
            {
                _context.ChangeTracker.Clear();

                var currentRecordCount = _context.Wizyty.Count();

                var recentChanges = _context.Wizyty
                    .Where(w => w.GodzinaWejscia > _lastRefresh.AddSeconds(-30) ||
                               (w.GodzinaWyjscia.HasValue && w.GodzinaWyjscia > _lastRefresh.AddSeconds(-30)))
                    .Any();

                if (recentChanges || currentRecordCount != _lastRecordCount)
                {
                    OdswiezDane();
                    _lastRefresh = DateTime.Now;
                    _lastRecordCount = currentRecordCount;
                    this.Title = $"Rejestr Wizyt - Odświeżono: {DateTime.Now:HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                this.Title = "Rejestr Wizyt - Błąd połączenia";
                // Optionally log the error or show status
                ConnectionStatusTextBlock.Text = $"Status: Błąd auto-refresh - {ex.Message}";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void OdswiezDane()
        {
            if (_context == null) return;

            try
            {
                _context.ChangeTracker.Clear();

                var openVisits = _context.Wizyty
                    .Where(w => w.GodzinaWyjscia == null)
                    .OrderByDescending(w => w.GodzinaWejscia)
                    .AsNoTracking()
                    .ToList();

                WizytyOtwarteGrid.ItemsSource = openVisits;

                if (string.IsNullOrWhiteSpace(FiltrNazwiskoTextBox.Text) &&
                    string.IsNullOrWhiteSpace(FiltrDoKogoTextBox.Text) &&
                    !DataOdPicker.SelectedDate.HasValue &&
                    !DataDoPicker.SelectedDate.HasValue)
                {
                    var allVisits = _context.Wizyty
                        .OrderByDescending(w => w.GodzinaWejscia)
                        .AsNoTracking()
                        .ToList();

                    HistoriaGrid.ItemsSource = allVisits;
                }

                _lastRecordCount = _context.Wizyty.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd odświeżania danych: {ex.Message}");
            }
        }

        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            var serverIp = ServerIpTextBox.Text.Trim();
            var databaseName = DatabaseNameTextBox.Text.Trim();
            var instanceName = InstanceNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(serverIp) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(instanceName))
            {
                MessageBox.Show("Wypełnij wszystkie pola.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var testConnectionString = $"Server={serverIp}\\{instanceName};Database={databaseName};Trusted_Connection=True;Encrypt=False;";

            try
            {
                // Create completely new context for testing
                using (var testContext = new WizytyContext(testConnectionString))
                {
                    // Set a shorter timeout for testing
                    testContext.Database.SetCommandTimeout(5);

                    // Test the connection
                    var canConnect = testContext.Database.CanConnect();

                    if (canConnect)
                    {
                        ConnectionStatusTextBlock.Text = "Status: Test połączenia pomyślny";
                        ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                        MessageBox.Show("Połączenie z bazą danych zostało pomyślnie przetestowane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        ConnectionStatusTextBlock.Text = "Status: Test połączenia nieudany";
                        ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                        MessageBox.Show("Nie można połączyć się z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusTextBlock.Text = $"Status: Błąd testu - {ex.Message}";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Błąd połączenia: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var serverIp = ServerIpTextBox.Text.Trim();
            var databaseName = DatabaseNameTextBox.Text.Trim();
            var instanceName = InstanceNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(serverIp) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(instanceName))
            {
                MessageBox.Show("Wypełnij wszystkie pola.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // First test the connection before saving
            var newConnectionString = $"Server={serverIp}\\{instanceName};Database={databaseName};Trusted_Connection=True;Encrypt=False;";

            try
            {
                using (var testContext = new WizytyContext(newConnectionString))
                {
                    testContext.Database.SetCommandTimeout(5);
                    if (!testContext.Database.CanConnect())
                    {
                        MessageBox.Show("Nie można połączyć się z bazą danych z podanymi ustawieniami. Sprawdź parametry połączenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd testowania połączenia: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings.Remove("ServerIP");
                config.AppSettings.Settings.Add("ServerIP", serverIp);

                config.AppSettings.Settings.Remove("DatabaseName");
                config.AppSettings.Settings.Add("DatabaseName", databaseName);

                config.AppSettings.Settings.Remove("InstanceName");
                config.AppSettings.Settings.Add("InstanceName", instanceName);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                _currentConnectionString = newConnectionString;

                // Stop timer before recreating context
                _refreshTimer?.Stop();
                _context?.Dispose();

                InitializeDatabase();
                SetupAutoRefresh();

                ConnectionInfoTextBlock.Text = $"Aktualny connection string:\n{_currentConnectionString}";

                MessageBox.Show("Ustawienia zostały zapisane i połączenie zostało zaktualizowane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisywania ustawień: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ServerIpTextBox.Text = "localhost";
            DatabaseNameTextBox.Text = "RecepcjaFirma";
            InstanceNameTextBox.Text = "SQLEXPRESS";

            MessageBox.Show("Ustawienia zostały przywrócone do wartości domyślnych. Kliknij 'Zapisz ustawienia' aby zastosować.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DodajWejscie_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var w = new Wizyta
            {
                Imie = ImieTextBox.Text,
                Nazwisko = NazwiskoTextBox.Text,
                DoKogo = DoKogoTextBox.Text,
                GodzinaWejscia = DateTime.Now
            };
            _context.Wizyty.Add(w);
            _context.SaveChanges();

            // Clear input fields
            ImieTextBox.Clear();
            NazwiskoTextBox.Clear();
            DoKogoTextBox.Clear();

            OdswiezDane();
            _lastRefresh = DateTime.Now;
        }

        private void DodajWyjscie_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (WizytyOtwarteGrid.SelectedItem is Wizyta selectedWizyta)
            {
                // Find the entity in the database and update it
                var wizytaInDb = _context.Wizyty.FirstOrDefault(w => w.Id == selectedWizyta.Id);
                if (wizytaInDb != null)
                {
                    wizytaInDb.GodzinaWyjscia = DateTime.Now;
                    _context.SaveChanges();
                    OdswiezDane();
                    _lastRefresh = DateTime.Now;
                }
                else
                {
                    MessageBox.Show("Nie można znaleźć wizyty w bazie danych.");
                }
            }
            else
            {
                MessageBox.Show("Wybierz wizytę z listy 'Osoby w budynku'.");
            }
        }

        private void UsunWizyte_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (HistoriaGrid.SelectedItem is Wizyta selectedWizyta)
            {
                var result = MessageBox.Show($"Czy na pewno chcesz usunąć wizytę {selectedWizyta.Imie} {selectedWizyta.Nazwisko}?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Find the entity in the database and remove it
                    var wizytaInDb = _context.Wizyty.FirstOrDefault(w => w.Id == selectedWizyta.Id);
                    if (wizytaInDb != null)
                    {
                        _context.Wizyty.Remove(wizytaInDb);
                        _context.SaveChanges();
                        OdswiezDane();
                        _lastRefresh = DateTime.Now;
                        MessageBox.Show("Wizyta została usunięta.");
                    }
                    else
                    {
                        MessageBox.Show("Nie można znaleźć wizyty w bazie danych.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Wybierz wizytę do usunięcia.");
            }
        }

        private void FiltrujHistorie_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var query = _context.Wizyty.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltrNazwiskoTextBox.Text))
            {
                query = query.Where(w => w.Nazwisko.Contains(FiltrNazwiskoTextBox.Text));
            }

            if (DataOdPicker.SelectedDate.HasValue)
            {
                query = query.Where(w => w.GodzinaWejscia >= DataOdPicker.SelectedDate.Value);
            }

            if (DataDoPicker.SelectedDate.HasValue)
            {
                query = query.Where(w => w.GodzinaWejscia <= DataDoPicker.SelectedDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(FiltrDoKogoTextBox.Text))
            {
                query = query.Where(w => w.DoKogo.Contains(FiltrDoKogoTextBox.Text));
            }

            HistoriaGrid.ItemsSource = query.OrderByDescending(w => w.GodzinaWejscia).ToList();
        }

        private void EksportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (HistoriaGrid.ItemsSource == null)
            {
                MessageBox.Show("Brak danych do eksportu.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"wizyty_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
            var filePath = Path.Combine(downloadsPath, fileName);

            var sb = new StringBuilder();
            sb.AppendLine("Imię,Nazwisko,Do kogo,Wejście,Wyjście,Czas trwania");

            // Export only filtered data from HistoriaGrid
            foreach (var w in HistoriaGrid.ItemsSource.Cast<Wizyta>())
            {
                var czasTrwania = w.GodzinaWyjscia.HasValue ?
                    (w.GodzinaWyjscia.Value - w.GodzinaWejscia).ToString(@"hh\:mm\:ss") :
                    "W trakcie";

                var wyjscie = w.GodzinaWyjscia?.ToString("dd.MM.yyyy HH:mm:ss") ?? "W trakcie";

                sb.AppendLine($"\"{w.Imie}\",\"{w.Nazwisko}\",\"{w.DoKogo}\",\"{w.GodzinaWejscia:dd.MM.yyyy HH:mm:ss}\",\"{wyjscie}\",\"{czasTrwania}\"");
            }

            // Save with UTF-8 encoding to handle Polish characters
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Zapisano CSV w: {filePath}");
        }

        private void EksportPdf_Click(object sender, RoutedEventArgs e)
        {
            if (HistoriaGrid.ItemsSource == null)
            {
                MessageBox.Show("Brak danych do eksportu.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var fileName = $"wizyty_export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf";
            var filePath = Path.Combine(downloadsPath, fileName);

            var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 10);
            var boldFont = new XFont("Arial", 12, XFontStyle.Bold);

            // Title
            gfx.DrawString("Rejestr Wizyt", boldFont, XBrushes.Black, new XPoint(40, 30));
            gfx.DrawString($"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm:ss}", font, XBrushes.Gray, new XPoint(40, 50));

            int y = 80;
            // Export only filtered data from HistoriaGrid
            foreach (var w in HistoriaGrid.ItemsSource.Cast<Wizyta>())
            {
                var wyjscie = w.GodzinaWyjscia?.ToString("dd.MM.yyyy HH:mm:ss") ?? "W trakcie";
                var czasTrwania = w.GodzinaWyjscia.HasValue ?
                    (w.GodzinaWyjscia.Value - w.GodzinaWejscia).ToString(@"hh\:mm\:ss") :
                    "W trakcie";

                var line = $"{w.Imie} {w.Nazwisko} | Do: {w.DoKogo} | Wejście: {w.GodzinaWejscia:dd.MM.yyyy HH:mm:ss} | Wyjście: {wyjscie} | Czas: {czasTrwania}";

                gfx.DrawString(line, font, XBrushes.Black, new XPoint(40, y));
                y += 15;

                // Add new page if needed
                if (y > 750)
                {
                    page = doc.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            doc.Save(filePath);
            MessageBox.Show($"Zapisano PDF w: {filePath}");
        }

        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Stop();
            _context?.Dispose();
            base.OnClosed(e);
        }
    }

    public class Wizyta
    {
        public int Id { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string DoKogo { get; set; }
        public DateTime GodzinaWejscia { get; set; }
        public DateTime? GodzinaWyjscia { get; set; }
    }

    public class WizytyContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _connectionString;

        public Microsoft.EntityFrameworkCore.DbSet<Wizyta> Wizyty { get; set; }

        // Konstruktor domyślny
        public WizytyContext()
        {
            _connectionString = "Server=localhost\\SQLEXPRESS;Database=RecepcjaFirma;Trusted_Connection=True;Encrypt=False;";
        }

        // Konstruktor z parametrem connection string
        public WizytyContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Konstruktor dla DbContextOptions
        public WizytyContext(DbContextOptions<WizytyContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }
    }
}