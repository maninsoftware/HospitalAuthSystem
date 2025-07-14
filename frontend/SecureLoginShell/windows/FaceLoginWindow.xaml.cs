using HospitalLoginApp.Helpers;
using HospitalLoginApp.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
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

            int elapsed = 0;
            int timeout = 5000;
            int interval = 100;

            while (elapsed < timeout)
            {
                if (webcamHelper.BlinkOccurred)
                {
                    lblStatus.Text = "✅ Blink detected!";
                    break;
                }

                await TPL.Task.Delay(interval);
                elapsed += interval;
            }

            if (!webcamHelper.BlinkOccurred)
            {
                lblStatus.Text = "❌ Blink not detected in time.";
                return;
            }

            lblStatus.Text = "📸 Capturing image...";
            byte[]? imageBytes = webcamHelper.CaptureImage(forceCapture: true);

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
                await TPL.Task.Delay(1000);
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
