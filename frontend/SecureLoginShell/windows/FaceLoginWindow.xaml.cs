using System;
using System.Threading.Tasks;
using System.Windows;
using HospitalLoginApp.Helpers;
using HospitalLoginApp.Services;

namespace HospitalLoginApp.Windows
{
    public partial class FaceLoginWindow : Window
    {
        private WebcamHelper webcamHelper;

        public FaceLoginWindow()
        {
            InitializeComponent();
            webcamHelper = new WebcamHelper(imgWebcam, Dispatcher);
            webcamHelper.StartPreview();
            lblStatus.Text = "📷 Webcam preview started.";
        }

        private async void BtnFaceVerify_Click(object sender, RoutedEventArgs e)
        {
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
                lblStatus.Text = $"✅ Welcome, {username}!";
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

        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.Dispose();
            base.OnClosed(e);
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Simply close this popup and return to main window
        }
    }
}
