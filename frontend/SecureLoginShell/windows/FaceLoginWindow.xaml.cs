using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using HospitalLoginApp.Helpers;
using HospitalLoginApp.Services;
using TPL = System.Threading.Tasks;

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
            if (webcamHelper == null)
            {
                lblStatus.Text = "❌ Webcam not initialized.";
                return;
            }

            lblStatus.Text = "👀 Please blink within 5 seconds...";
            webcamHelper.ResetLiveness();
            webcamHelper.ActivateLivenessCheck();

            await TPL.Task.Delay(5000); // Wait for user to blink
            await TPL.Task.Delay(300);  // Slight buffer

            bool isLive = webcamHelper.IsFaceLive();
            if (!isLive)
            {
                lblStatus.Text = "❌ Face not live or capture failed.";
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
                await TPL.Task.Delay(2000);
                ShellHelper.LaunchWindowsShellIfNeeded();
                Application.Current.Shutdown();
            }
            else
            {
                lblStatus.Text = "❌ Face not recognized. Please try again.";
                webcamHelper.StartPreview();
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            webcamHelper?.StopPreview();
            webcamHelper?.Dispose();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            webcamHelper?.Dispose();
            base.OnClosed(e);
        }
    }
}
