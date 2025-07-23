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
using System.Windows.Controls;
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
        private User _currentUser;
        private bool _czyWczytanoCzcionke = false;

        public MainWindow()
        {
            InitializeComponent();
            ShowLoginDialog();
        }

        private void ShowLoginDialog()
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result == true && loginWindow.LoginSuccessful)
            {
                _currentUser = loginWindow.LoggedInUser;
                this.Show(); 
                InitializeAfterLogin();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }


        private void InitializeAfterLogin()
        {
            // Aktualizuj UI na podstawie zalogowanego użytkownika
            CurrentUserTextBlock.Text = $"Użytkownik: {_currentUser.Username} ({(_currentUser.IsAdmin ? "Administrator" : "Użytkownik")})";
            LogoutButton.IsEnabled = true;

            // Ukryj/pokaż elementy w zależności od uprawnień
            

            LoadSettings();
            InitializeDatabase();
            LoadSettingsToUI();
            SetupAutoRefresh();
            LoadUsers();
            LoadLogs();
            LoadFontSize();
            SetUIPermissions();
        }

        private void SetUIPermissions()
        {
            // Tylko administratorzy mogą usuwać wizyty
            UsunWizyteButton.IsEnabled = _currentUser.IsAdmin;

            // Tylko administratorzy mogą czyścić logi
            ClearLogsButton.IsEnabled = _currentUser.IsAdmin;

            var tabControl = this.FindName("TabControl") as TabControl;
            var usersTab = this.FindName("UsersTab") as TabItem;
            if (tabControl != null && usersTab != null && !_currentUser.IsAdmin)
            {
                tabControl.Items.Remove(usersTab);
            }
        }

        private void LoadFontSize()
        {
            double fontSize = Properties.Settings.Default.FontSize;
            FontSizeSlider.Value = fontSize;
            ApplyFontSize(fontSize);

            Dispatcher.InvokeAsync(() =>
            {
                if (FontSizeValue != null)
                    FontSizeValue.Text = ((int)fontSize).ToString();
            });

            _czyWczytanoCzcionke = true;
        }





        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_czyWczytanoCzcionke)
                return;

            var fontSize = e.NewValue;

            if (FontSizeValue != null)
                FontSizeValue.Text = ((int)fontSize).ToString();

            ApplyFontSize(fontSize);
            SaveFontSize(fontSize);
        }



        private void ApplyFontSize(double fontSize)
        {
            this.FontSize = fontSize;

            if (WizytyOtwarteGrid != null)
                WizytyOtwarteGrid.FontSize = fontSize;

            if (HistoriaGrid != null)
                HistoriaGrid.FontSize = fontSize;

            if (LogsGrid != null)
                LogsGrid.FontSize = fontSize;

            if (UsersGrid != null)
                UsersGrid.FontSize = fontSize;
        }


        private void SaveFontSize(double fontSize)
        {
            try
            {
                Properties.Settings.Default.FontSize = fontSize;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu: {ex.Message}");
            }
        }



        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Loguj wylogowanie
            LogAction("Wylogowanie", "Użytkownik wylogował się z systemu");

            // Zatrzymaj timer i zamknij połączenie
            _refreshTimer?.Stop();
            _context?.Dispose();

            // Ukryj główne okno i pokaż okno logowania
            this.Hide();
            ShowLoginDialog();
        }

        private void LoadUsers()
        {
            if (_context == null) return;

            try
            {
                var users = _context.Users.Select(u => new
                {
                    u.Id,
                    u.Username,
                    Role = u.IsAdmin ? "Administrator" : "Użytkownik",
                    u.CreatedAt
                }).ToList();

                UsersGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania użytkowników: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.IsAdmin)
            {
                MessageBox.Show("Tylko administratorzy mogą dodawać użytkowników.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var username = NewUsernameTextBox.Text.Trim();
            var password = NewPasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Wypełnij wszystkie pola.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Hasło musi mieć co najmniej 6 znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Sprawdź, czy użytkownik już istnieje
                if (_context.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Użytkownik o tej nazwie już istnieje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = LoginWindow.HashPassword(password),
                    IsAdmin = IsAdminCheckBox.IsChecked == true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                // Wyczyść pola
                NewUsernameTextBox.Clear();
                NewPasswordBox.Clear();
                IsAdminCheckBox.IsChecked = false;

                LoadUsers();
                LogAction("Dodanie użytkownika", $"Dodano nowego użytkownika: {username}");

                MessageBox.Show($"Użytkownik {username} został dodany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd dodawania użytkownika: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.IsAdmin)
            {
                MessageBox.Show("Tylko administratorzy mogą usuwać użytkowników.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var button = sender as Button;
            if (button?.Tag == null) return;

            var userId = Convert.ToInt32(button.Tag);

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null)
                {
                    MessageBox.Show("Nie można znaleźć użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Nie pozwól usunąć siebie
                if (user.Id == _currentUser.Id)
                {
                    MessageBox.Show("Nie możesz usunąć swojego własnego konta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Czy na pewno chcesz usunąć użytkownika {user.Username}?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();

                    LoadUsers();
                    LogAction("Usunięcie użytkownika", $"Usunięto użytkownika: {user.Username}");

                    MessageBox.Show($"Użytkownik {user.Username} został usunięty.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd usuwania użytkownika: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLogs()
        {
            if (_context == null) return;

            try
            {
                var logs = _context.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(1000) // Ogranicz do ostatnich 1000 logów
                    .ToList();

                LogsGrid.ItemsSource = logs;
                LogsStatusTextBlock.Text = $"Załadowano {logs.Count} logów";
            }
            catch (Exception ex)
            {
                LogsStatusTextBlock.Text = $"Błąd ładowania logów: {ex.Message}";
            }
        }

        private void RefreshLogs_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            if (!_currentUser.IsAdmin)
            {
                MessageBox.Show("Tylko administratorzy mogą czyścić logi.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_context == null)
            {
                MessageBox.Show("Brak połączenia z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Czy na pewno chcesz wyczyścić wszystkie logi? Ta operacja jest nieodwracalna.",
                "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var allLogs = _context.Logs.ToList();
                    _context.Logs.RemoveRange(allLogs);
                    _context.SaveChanges();

                    // Dodaj log o wyczyszczeniu logów
                    LogAction("Wyczyszczenie logów", "Wszystkie logi zostały wyczyszczone");

                    LoadLogs();
                    MessageBox.Show("Logi zostały wyczyszczone.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd czyszczenia logów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogLevelFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_context == null || LogLevelFilter.SelectedItem == null) return;

            try
            {
                var selectedLevel = (LogLevelFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

                IQueryable<Log> query = _context.Logs.OrderByDescending(l => l.Timestamp);

                if (selectedLevel != "Wszystkie")
                {
                    query = query.Where(l => l.Level == selectedLevel);
                }

                var filteredLogs = query.Take(1000).ToList();
                LogsGrid.ItemsSource = filteredLogs;
                LogsStatusTextBlock.Text = $"Załadowano {filteredLogs.Count} logów (filtr: {selectedLevel})";
            }
            catch (Exception ex)
            {
                LogsStatusTextBlock.Text = $"Błąd filtrowania logów: {ex.Message}";
            }
        }

        private void LogAction(string action, string details, string level = "Info")
        {
            if (_context == null) return;

            try
            {
                var log = new Log
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Username = _currentUser?.Username ?? "System",
                    Action = action,
                    Details = details
                };

                _context.Logs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Logowanie nie powinno przerywać normalnego działania aplikacji
                // Można ewentualnie wyświetlić komunikat w status bar
                LogsStatusTextBlock.Text = $"Błąd logowania: {ex.Message}";
            }
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
                _context?.Dispose();
                _context = new WizytyContext(_currentConnectionString);
                _context.Database.EnsureCreated();
                OdswiezDane();
                ConnectionStatusTextBlock.Text = "Status: Połączono pomyślnie";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;

                LogAction("Połączenie z bazą", "Pomyślnie połączono z bazą danych");
            }
            catch (Exception ex)
            {
                _context?.Dispose();
                _context = null;
                ConnectionStatusTextBlock.Text = $"Status: Błąd połączenia - {ex.Message}";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Błąd połączenia z bazą danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                LogAction("Błąd połączenia", $"Błąd połączenia z bazą danych: {ex.Message}", "Error");
            }
        }

        private void SetupAutoRefresh()
        {
            _refreshTimer?.Stop();
            _refreshTimer = new System.Windows.Threading.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(15);
            _refreshTimer.Tick += AutoRefresh_Tick;
            _refreshTimer.Start();
        }

        private void AutoRefresh_Tick(object sender, EventArgs e)
        {
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

                // Otwarte wizyty
                var openVisits = _context.Wizyty
                    .Where(w => w.GodzinaWyjscia == null)
                    .OrderByDescending(w => w.GodzinaWejscia)
                    .AsNoTracking()
                    .ToList()
                    .Select(w => new Wizyta
                    {
                        Id = w.Id,
                        Imie = w.Imie ?? "",
                        Nazwisko = w.Nazwisko ?? "",
                        DoKogo = w.DoKogo ?? "",
                        Firma = w.Firma ?? "",
                        Uwagi = w.Uwagi ?? "",
                        GodzinaWejscia = w.GodzinaWejscia,
                        GodzinaWyjscia = w.GodzinaWyjscia
                    })
                    .ToList();

                WizytyOtwarteGrid.ItemsSource = openVisits;

                // Historia (jeśli brak filtrów)
                if (string.IsNullOrWhiteSpace(FiltrNazwiskoTextBox.Text) &&
                    string.IsNullOrWhiteSpace(FiltrDoKogoTextBox.Text) &&
                    string.IsNullOrWhiteSpace(FiltrFirmaTextBox.Text) &&
                    !DataOdPicker.SelectedDate.HasValue &&
                    !DataDoPicker.SelectedDate.HasValue)
                {
                    var allVisits = _context.Wizyty
                        .OrderByDescending(w => w.GodzinaWejscia)
                        .AsNoTracking()
                        .ToList()
                        .Select(w => new Wizyta
                        {
                            Id = w.Id,
                            Imie = w.Imie ?? "",
                            Nazwisko = w.Nazwisko ?? "",
                            DoKogo = w.DoKogo ?? "",
                            Firma = w.Firma ?? "",
                            Uwagi = w.Uwagi ?? "",
                            GodzinaWejscia = w.GodzinaWejscia,
                            GodzinaWyjscia = w.GodzinaWyjscia
                        })
                        .ToList();

                    HistoriaGrid.ItemsSource = allVisits;
                }

                _lastRecordCount = _context.Wizyty.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd odświeżania danych: {ex.Message}");
                LogAction("Błąd odświeżania", $"Błąd odświeżania danych: {ex.Message}", "Error");
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
                using (var testContext = new WizytyContext(testConnectionString))
                {
                    testContext.Database.SetCommandTimeout(5);
                    var canConnect = testContext.Database.CanConnect();

                    if (canConnect)
                    {
                        ConnectionStatusTextBlock.Text = "Status: Test połączenia pomyślny";
                        ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                        MessageBox.Show("Połączenie z bazą danych zostało pomyślnie przetestowane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        LogAction("Test połączenia", "Test połączenia zakończony pomyślnie");
                    }
                    else
                    {
                        ConnectionStatusTextBlock.Text = "Status: Test połączenia nieudany";
                        ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                        MessageBox.Show("Nie można połączyć się z bazą danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        LogAction("Test połączenia", "Test połączenia nieudany", "Warning");
                    }
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusTextBlock.Text = $"Status: Błąd testu - {ex.Message}";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Błąd połączenia: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                LogAction("Test połączenia", $"Błąd testu połączenia: {ex.Message}", "Error");
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

                _refreshTimer?.Stop();
                _context?.Dispose();

                InitializeDatabase();
                SetupAutoRefresh();
                LoadUsers();
                LoadLogs();

                ConnectionInfoTextBlock.Text = $"Aktualny connection string:\n{_currentConnectionString}";

                MessageBox.Show("Ustawienia zostały zapisane i połączenie zostało zaktualizowane!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                LogAction("Zapisanie ustawień", "Ustawienia połączenia zostały zaktualizowane");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisywania ustawień: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                LogAction("Błąd zapisu ustawień", $"Błąd zapisywania ustawień: {ex.Message}", "Error");
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

            var imie = ImieTextBox.Text.Trim();
            var nazwisko = NazwiskoTextBox.Text.Trim();
            var doKogo = DoKogoTextBox.Text.Trim();
            var firma = FirmaTextBox.Text.Trim();
            var uwagi = UwagiTextBox.Text.Trim();

            if (string.IsNullOrEmpty(nazwisko))
            {
                MessageBox.Show("Pole 'Nazwisko' jest wymagane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var w = new Wizyta
            {
                Imie = imie,
                Nazwisko = nazwisko,
                DoKogo = doKogo,
                Firma = firma,
                Uwagi = uwagi,
                GodzinaWejscia = DateTime.Now
            };

            _context.Wizyty.Add(w);
            _context.SaveChanges();

            // Clear input fields
            ImieTextBox.Clear();
            NazwiskoTextBox.Clear();
            DoKogoTextBox.Clear();
            FirmaTextBox.Clear();
            UwagiTextBox.Clear();

            OdswiezDane();
            _lastRefresh = DateTime.Now;

            LogAction("Dodanie wejścia", $"Dodano wejście: {imie} {nazwisko} do {doKogo}");
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
                var wizytaInDb = _context.Wizyty.FirstOrDefault(w => w.Id == selectedWizyta.Id);
                if (wizytaInDb != null)
                {
                    wizytaInDb.GodzinaWyjscia = DateTime.Now;
                    _context.SaveChanges();
                    OdswiezDane();
                    _lastRefresh = DateTime.Now;

                    LogAction("Dodanie wyjścia", $"Dodano wyjście: {selectedWizyta.Imie} {selectedWizyta.Nazwisko}");
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
            if (!_currentUser.IsAdmin)
            {
                MessageBox.Show("Tylko administratorzy mogą usuwać wizyty.", "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    var wizytaInDb = _context.Wizyty.FirstOrDefault(w => w.Id == selectedWizyta.Id);
                    if (wizytaInDb != null)
                    {
                        _context.Wizyty.Remove(wizytaInDb);
                        _context.SaveChanges();
                        OdswiezDane();
                        _lastRefresh = DateTime.Now;

                        LogAction("Usunięcie wizyty", $"Usunięto wizytę: {selectedWizyta.Imie} {selectedWizyta.Nazwisko}");
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

            if (!string.IsNullOrWhiteSpace(FiltrFirmaTextBox.Text))
            {
                query = query.Where(w => (w.Firma ?? "").Contains(FiltrFirmaTextBox.Text));
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
            sb.AppendLine("Imię,Nazwisko,Do kogo,Firma,Uwagi,Wejście,Wyjście,Czas trwania");

            // Export only filtered data from HistoriaGrid
            foreach (var w in HistoriaGrid.ItemsSource.Cast<Wizyta>())
            {
                var czasTrwania = w.GodzinaWyjscia.HasValue ?
                    (w.GodzinaWyjscia.Value - w.GodzinaWejscia).ToString(@"hh\:mm\:ss") :
                    "W trakcie";

                var wyjscie = w.GodzinaWyjscia?.ToString("dd.MM.yyyy HH:mm:ss") ?? "W trakcie";

                sb.AppendLine($"\"{w.Imie}\",\"{w.Nazwisko}\",\"{w.DoKogo}\",\"{w.Firma}\",\"{w.Uwagi}\",\"{w.GodzinaWejscia:dd.MM.yyyy HH:mm:ss}\",\"{wyjscie}\",\"{czasTrwania}\"");
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

                var line = $"{w.Imie} {w.Nazwisko} | Firma: {w.Firma} | Do: {w.DoKogo} | Wejście: {w.GodzinaWejscia:dd.MM.yyyy HH:mm:ss} | Wyjście: {wyjscie} | Czas: {czasTrwania}";
                var uwagiLine = $"Uwagi: {w.Uwagi}";

                gfx.DrawString(line, font, XBrushes.Black, new XPoint(40, y));
                y += 15;

                gfx.DrawString(uwagiLine, font, XBrushes.DarkGray, new XPoint(60, y));
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
        public string Firma { get; set; }
        public string Uwagi { get; set; }
        public DateTime GodzinaWejscia { get; set; }
        public DateTime? GodzinaWyjscia { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Log
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } // Info, Warning, Error
        public string Username { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }


    public class WizytyContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _connectionString;

        public Microsoft.EntityFrameworkCore.DbSet<Wizyta> Wizyty { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }


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