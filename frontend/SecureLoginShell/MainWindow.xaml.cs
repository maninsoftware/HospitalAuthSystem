using HospitalLoginApp.Services;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MediaColor = System.Windows.Media.Color;
using HospitalLoginApp.Helpers;


namespace HospitalLoginApp
{
    public partial class MainWindow : Window
    {
        private WebcamHelper? webcamHelper;

        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0));
            txtPassword.Password = "Password";
            txtPassword.Tag = "placeholder";
            txtPassword.Foreground = new SolidColorBrush(Colors.Gray);
        }

        // Credential Login
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
                lblStatus.Text = $"✅ Welcome, {username}!";
                // Launch full Windows shell only if not running
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();  // Close the fullscreen login app
            }
            else
            {
                lblStatus.Text = "❌ Login Failed!";
            }
        }

        // Face Login
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

                // ✅ Delay to allow user to see welcome message
                await Task.Delay(2000); // 2 seconds
                // Launch full Windows shell only if not running
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();  // Exit the login window
            }
            else
            {
                lblStatus.Text = "❌ Face not recognized. Please try again.";
                webcamHelper.StartPreview();
            }
        }

        // Registration
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
                BtnCredentialMode_Click(this, new RoutedEventArgs());

            }
            else
            {
                lblStatus.Text = "❌ Registration failed.";
            }
        }

        // Toggle Panels
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

            webcamHelper?.StopPreview();
            webcamHelper = new WebcamHelper(imgWebcam, Dispatcher);
            webcamHelper.StartPreview();
            lblStatus.Text = "📷 Live preview started.";
        }

        private void BtnRegisterMode_Click(object sender, RoutedEventArgs e)
        {
            CredentialPanel.Visibility = Visibility.Collapsed;
            FacePanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;

            webcamHelper?.StopPreview();
            webcamHelper = new WebcamHelper(regWebcam, Dispatcher);
            webcamHelper.StartPreview();
            lblStatus.Text = "📷 Prepare for registration photo.";
        }
        private void ClearText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text == "Username")
                textBox.Text = "";
        }

        private void RestoreText(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                textBox.Text = "Username";
        }
        private void ClearPassword(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pwdBox && pwdBox.Tag?.ToString() == "placeholder")
            {
                pwdBox.Clear();
                pwdBox.Tag = null;
                pwdBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void RestorePassword(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pwdBox && string.IsNullOrWhiteSpace(pwdBox.Password))
            {
                pwdBox.Password = "Password";
                pwdBox.Tag = "placeholder";
                pwdBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.StopPreview();
            base.OnClosed(e);
        }
    }
}
