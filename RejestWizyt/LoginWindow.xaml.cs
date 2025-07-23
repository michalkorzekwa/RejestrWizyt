using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace RejestWizyt
{
    public partial class LoginWindow : Window
    {
        private WizytyContext _context;
        public User LoggedInUser { get; private set; }
        public bool LoginSuccessful { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            LoadConnectionString();
            InitializeDatabase();

            // Enable Enter key to login
            PasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) Login_Click(null, null); };
            UsernameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) PasswordBox.Focus(); };

            // Focus on username textbox
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void LoadConnectionString()
        {
            try
            {
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                var serverIp = config.AppSettings.Settings["ServerIP"]?.Value ?? "localhost";
                var databaseName = config.AppSettings.Settings["DatabaseName"]?.Value ?? "RecepcjaFirma";
                var instanceName = config.AppSettings.Settings["InstanceName"]?.Value ?? "SQLEXPRESS";

                var connectionString = $"Server={serverIp}\\{instanceName};Database={databaseName};Trusted_Connection=True;Encrypt=False;";
                _context = new WizytyContext(connectionString);
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Błąd połączenia z bazą danych: {ex.Message}";
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                _context?.Database.EnsureCreated();

                // Create default admin user if no users exist
                if (!_context.Users.Any())
                {
                    var defaultAdmin = new User
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        IsAdmin = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.Users.Add(defaultAdmin);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Błąd inicjalizacji bazy danych: {ex.Message}";
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (_context == null)
            {
                ErrorTextBlock.Text = "Brak połączenia z bazą danych.";
                return;
            }

            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Wypełnij wszystkie pola.";
                return;
            }

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);

                if (user != null && VerifyPassword(password, user.PasswordHash))
                {
                    LoggedInUser = user;
                    LoginSuccessful = true;

                    // Log successful login
                    var log = new Log
                    {
                        Timestamp = DateTime.Now,
                        Level = "Info",
                        Username = username,
                        Action = "Logowanie",
                        Details = "Pomyślne logowanie do systemu"
                    };
                    _context.Logs.Add(log);
                    _context.SaveChanges();

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ErrorTextBlock.Text = "Nieprawidłowa nazwa użytkownika lub hasło.";

                    // Log failed login attempt
                    var log = new Log
                    {
                        Timestamp = DateTime.Now,
                        Level = "Warning",
                        Username = username,
                        Action = "Nieudane logowanie",
                        Details = "Próba logowania z nieprawidłowymi danymi"
                    };
                    _context.Logs.Add(log);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = $"Błąd logowania: {ex.Message}";
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "RejestWizytSalt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }
    }
}