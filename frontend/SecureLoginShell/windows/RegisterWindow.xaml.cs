using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using HospitalLoginApp.Helpers;
using HospitalLoginApp.Services;
using System.Security.Principal;
using TPL = System.Threading.Tasks;

namespace HospitalLoginApp.Windows
{
    public partial class RegisterWindow : Window
    {
        private WebcamHelper webcamHelper;

        public RegisterWindow()
        {
            InitializeComponent();
            //this.Topmost = true;
            //this.WindowState = WindowState.Maximized;
            //this.WindowStyle = WindowStyle.None;
            //this.AllowsTransparency = true;
            //this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0)); // 50% black

            // Initialize webcam preview
            webcamHelper = new WebcamHelper(regWebcam, Dispatcher);
            webcamHelper.StartPreview();
            lblStatus.Text = "📸 Webcam ready for registration.";

            // Set placeholders
            regUsername.Text = "Username";
            regUsername.Foreground = Brushes.Gray;

            regPassword.Password = "Password";
            regPassword.Foreground = Brushes.Gray;
            regPassword.Tag = "placeholder";
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = regUsername.Text.Trim();
            string password = regPassword.Password;

            if (username == "Username" || password == "Password" ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "⚠️ Enter both username and password.";
                return;
            }

            lblStatus.Text = "👁️ Please blink to verify liveness...";

            // Activate liveness check and wait for the window to end
            webcamHelper.ActivateLivenessCheck();

            // Wait for liveness window (5 seconds is default in WebcamHelper)
            await Task.Delay(5500);

            if (!webcamHelper.IsFaceLive())
            {
                lblStatus.Text = "❌ Liveness check failed. Please try again.";
                return;
            }

            lblStatus.Text = "📸 Capturing image...";
            byte[]? imageBytes = webcamHelper.CaptureImage();

            if (imageBytes == null)
            {
                lblStatus.Text = "❌ Failed to capture image.";
                return;
            }

            lblStatus.Text = "📝 Registering...";
            string? response = await ApiService.RegisterUser(username, password, imageBytes);

            if (!string.IsNullOrEmpty(response))
            {
                lblStatus.Text = $"✅ {response}";
                webcamHelper.StopPreview();
                await Task.Delay(2000);
                this.Close(); // Close the register window
            }
            else
            {
                lblStatus.Text = "❌ Registration failed.";
            }
        }



        // Handle cleanup on close
        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.Dispose();
            base.OnClosed(e);
        }

        // Manual Placeholder Handlers
        private void regUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            if (regUsername.Text == "Username")
            {
                regUsername.Text = "";
                regUsername.Foreground = Brushes.Black;
            }
        }

        private void regUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(regUsername.Text))
            {
                regUsername.Text = "Username";
                regUsername.Foreground = Brushes.Gray;
            }
        }

        private void regPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (regPassword.Password == "Password" && regPassword.Tag?.ToString() == "placeholder")
            {
                regPassword.Clear();
                regPassword.Foreground = Brushes.Black;
                regPassword.Tag = null;
            }
        }

        private void regPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(regPassword.Password))
            {
                regPassword.Password = "Password";
                regPassword.Foreground = Brushes.Gray;
                regPassword.Tag = "placeholder";
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            webcamHelper?.StopPreview();   // Stop the camera preview
            webcamHelper?.Dispose();
            this.Close(); // Simply close this popup and return to main window
        }
      
    }
}
