using System;
using System.Windows;
using System.Threading.Tasks;
using HospitalLoginApp.Services;
using System.Windows.Media;
using HospitalLoginApp.Helpers;
namespace HospitalLoginApp.Windows
{
    public partial class CredentialLoginWindow : Window
    {
        public CredentialLoginWindow()
        {
            InitializeComponent();
        }
        private void txtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == "Username")
            {
                txtUsername.Text = "";
                txtUsername.Foreground = Brushes.Black;
            }
        }

        private void txtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = "Username";
                txtUsername.Foreground = Brushes.Gray;
            }
        }

        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password == "Password" && txtPassword.Tag?.ToString() == "placeholder")
            {
                txtPassword.Clear();
                txtPassword.Foreground = Brushes.Black;
                txtPassword.Tag = null;
            }
        }

        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                txtPassword.Password = "Password";
                txtPassword.Foreground = Brushes.Gray;
                txtPassword.Tag = "placeholder";
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Simply close this popup and return to main window
        }
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "⚠️ Please enter both username and password.";
                return;
            }

            lblStatus.Text = "🔐 Verifying...";
            bool isValid = await ApiService.VerifyCredentials(username, password);

            if (isValid)
            {
                lblStatus.Text = $"✅ Welcome, {username}!";
                await Task.Delay(1500);

                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown(); // Ends the entire app
            }
            else
            {
                lblStatus.Text = "❌ Invalid credentials.";
            }
        }
    }
}
