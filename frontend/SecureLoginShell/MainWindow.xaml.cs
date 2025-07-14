using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using HospitalLoginApp.Windows;
using Microsoft.Win32.TaskScheduler;
using Microsoft.Identity.Client.Desktop;

namespace HospitalLoginApp
{


    public partial class MainWindow : Window
    {
        private IPublicClientApplication _clientApp;
        private readonly string[] _scopes = new[] { "user.read" };

        public MainWindow()
        {
            InitializeComponent();
            AuthenticateWithAzureAD(); // Begin authentication on startup
        }

        private async void AuthenticateWithAzureAD()
        {
            _clientApp = PublicClientApplicationBuilder
                .Create("4ed8f6b6-13f2-4440-aa6f-67b281f1ac2e") // ✅ Your App ID
                .WithAuthority(AzureCloudInstance.AzurePublic, "a690be8b-13a7-4b24-90b0-9ebe84f3d47d") // ✅ Your Tenant ID
                .WithRedirectUri("http://localhost") // ✅ Must match Azure redirect URI
                .WithWindowsEmbeddedBrowserSupport() // ✅ Use embedded browser
                .Build();

            try
            {
                var result = await _clientApp
                    .AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .WithUseEmbeddedWebView(true)
                    .ExecuteAsync();

                // ✅ Show UI
                var userEmail = result.Account.Username;
                var userName = userEmail.Split('@')[0];
                lblWelcome.Text = $"Welcome, {userName}";
                MainCard.Visibility = Visibility.Visible;
            }
            catch (MsalException ex)
            {
                MessageBox.Show("Azure AD login failed:\n" + ex.Message, "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void BtnCredentialLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new CredentialLoginWindow { Owner = this };
            this.Hide();
            window.ShowDialog();
            this.Show();
        }

        private void BtnFaceLogin_Click(object sender, RoutedEventArgs e)
        {
            var window = new FaceLoginWindow { Owner = this };
            this.Hide();
            window.ShowDialog();
            this.Show();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var window = new RegisterWindow { Owner = this };
            this.Hide();
            window.ShowDialog();
            this.Show();
        }
        private void RegisterTaskInTaskScheduler()
        {
            try
            {
                string appName = "HospitalLoginApp";
                string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? "";

                if (string.IsNullOrWhiteSpace(appPath) || !File.Exists(appPath))
                {
                    MessageBox.Show("❌ Could not locate application path.");
                    return;
                }

                using var ts = new TaskService();
                ts.RootFolder.DeleteTask(appName, false);

                var td = ts.NewTask();
                td.RegistrationInfo.Description = "Launch HospitalLoginApp at user logon";
                td.Principal.RunLevel = TaskRunLevel.Highest;
                td.Principal.LogonType = TaskLogonType.InteractiveToken;

                td.Triggers.Add(new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name });
                td.Actions.Add(new ExecAction(appPath, null, Path.GetDirectoryName(appPath)));

                ts.RootFolder.RegisterTaskDefinition(appName, td);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Task Scheduler Error: {ex.Message}");
            }
        }

        private void UnregisterTaskFromTaskScheduler()
        {
            try
            {
                using var ts = new TaskService();
                ts.RootFolder.DeleteTask("HospitalLoginApp", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to unregister task: {ex.Message}");
            }
        }
    }
}

