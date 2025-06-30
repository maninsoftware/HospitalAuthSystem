using HospitalLoginApp.Services;
using HospitalLoginApp.Helpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace HospitalLoginApp
{
    public partial class MainWindow : Window
    {
        private WebcamHelper? webcamHelper;

        public MainWindow()
        {
            InitializeComponent();
            //RegisterAppAtStartup(); // Register app to auto-start at login
            txtPassword.Password = "Password";
            txtPassword.Tag = "placeholder";
            txtPassword.Foreground = new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Registers the application to auto-launch on user logon via registry.
        /// </summary>
        private void RegisterAppAtStartup()
        {
            try
            {
                string appName = "HospitalLoginApp";
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                // Open the Run key safely and check for null
                RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                if (runKey != null)
                {
                    using (runKey)
                    {
                        runKey.SetValue(appName, $"\"{appPath}\"");
                    }
                }
                else
                {
                    MessageBox.Show("⚠️ Failed to access registry path: Run key is missing.", "Startup Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Failed to register app at startup.\n\n{ex.Message}", "Startup Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        /// <summary>
        /// Removes the application from startup registry key.
        /// Call this manually during uninstall or optional logout.
        /// </summary>
        private void UnregisterAppFromStartup()
        {
            try
            {
                string appName = "HospitalLoginApp";

                // Open the Run key safely and check for null
                RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
                if (runKey != null)
                {
                    using (runKey)
                    {
                        if (runKey.GetValue(appName) != null)
                        {
                            runKey.DeleteValue(appName);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("⚠️ Failed to access registry path: Run key is missing.", "Unregister Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Failed to unregister app from startup.\n\n{ex.Message}", "Unregister Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Please enter credentials.";
                return;
            }

            lblStatus.Text = "Verifying credentials...";
            bool result = await ApiService.VerifyCredentials(username, password);
            if (result)
            {
                lblStatus.Text = $"✅ Welcome, {username}! Loading your Homescreen...";
                await Task.Delay(2000);
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();
            }
            else
            {
                lblStatus.Text = "❌ Login Failed!";
            }
        }

        private async void BtnFaceVerify_Click(object sender, RoutedEventArgs e)
        {
            if (webcamHelper == null)
            {
                lblStatus.Text = "❌ Webcam not initialized.";
                return;
            }

            lblStatus.Text = "📸 Capturing image...";
            byte[]? imageBytes = webcamHelper.CaptureImage();

            if (imageBytes == null)
            {
                lblStatus.Text = "❌ Failed to capture image.";
                return;
            }

            lblStatus.Text = "🔍 Verifying face...";
            string? username = await ApiService.VerifyFace(imageBytes);

            if (!string.IsNullOrEmpty(username))
            {
                lblStatus.Text = $"✅ Welcome, {username}! Loading your Homescreen...";
                webcamHelper.StopPreview();
                await Task.Delay(2000);
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();
            }
            else
            {
                lblStatus.Text = "❌ Face not recognized. Please try again.";
                webcamHelper.StartPreview();
            }
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = regUsername.Text.Trim();
            string password = regPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Please enter username and password.";
                return;
            }

            lblStatus.Text = "📸 Capturing registration photo...";
            webcamHelper ??= new WebcamHelper(regWebcam, Dispatcher);
            byte[]? imageBytes = webcamHelper.CaptureImage();

            if (imageBytes == null)
            {
                lblStatus.Text = "❌ Failed to capture image.";
                return;
            }

            lblStatus.Text = "📝 Registering...";
            string? responseMessage = await ApiService.RegisterUser(username, password, imageBytes);

            if (!string.IsNullOrEmpty(responseMessage))
            {
                lblStatus.Text = $"✅ {responseMessage}";
                await Task.Delay(2000);
                BtnCredentialMode_Click(this, new RoutedEventArgs());
            }
            else
            {
                lblStatus.Text = "❌ Registration failed.";
            }
        }

        private void EnsureWebcamInitialized(Image target)
        {
            webcamHelper?.Dispose();
            webcamHelper = new WebcamHelper(target, Dispatcher);
            webcamHelper.StartPreview();
        }

        private void BtnCredentialMode_Click(object sender, RoutedEventArgs e)
        {
            CredentialPanel.Visibility = Visibility.Visible;
            FacePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Collapsed;

            webcamHelper?.StopPreview();
            lblStatus.Text = "";
        }

        private void BtnFaceMode_Click(object sender, RoutedEventArgs e)
        {
            CredentialPanel.Visibility = Visibility.Collapsed;
            FacePanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;

            EnsureWebcamInitialized(imgWebcam);
            lblStatus.Text = "📷 Live preview started.";
        }

        private void BtnRegisterMode_Click(object sender, RoutedEventArgs e)
        {
            CredentialPanel.Visibility = Visibility.Collapsed;
            FacePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;

            EnsureWebcamInitialized(regWebcam);
            lblStatus.Text = "📷 Prepare for registration photo.";
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text == "Username")
                tb.Text = "";
        }

        private void RestoreText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "Username";
        }

        private void ClearPassword(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb && pb.Tag?.ToString() == "placeholder")
            {
                pb.Clear();
                pb.Tag = null;
                pb.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void RestorePassword(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb && string.IsNullOrWhiteSpace(pb.Password))
            {
                pb.Password = "Password";
                pb.Tag = "placeholder";
                pb.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.Dispose();
            // Optional: uncomment if you want to remove startup on exit
            // UnregisterAppFromStartup();
            base.OnClosed(e);
        }
    }
}
